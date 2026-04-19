using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BusBookingSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace BusBookingSystem.Controllers
{
    public class LogInController : Controller
    {
        private readonly string _connection;

        public LogInController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string hashedInputPassword;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                hashedInputPassword = Convert.ToBase64String(hash);
            }

            using var conn = new MySqlConnection(_connection);
            conn.Open();

            string query = "SELECT Id, Username, Email FROM Users WHERE Email = @email AND PasswordHash = @password";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", hashedInputPassword);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userId = reader.GetInt32("Id");

                // ✅ FIXED (store INT)
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Username", reader.GetString("Username"));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}