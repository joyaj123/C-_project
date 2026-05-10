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

        public IActionResult Login() //tells ASP.NET what to send to the user 
        {
            return View();
        }

        [HttpPost] //when the user login
        public IActionResult Login(string email, string password)
        {

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
         {
              ViewBag.Error = "Email and password are required";
              return View();
         }
            string hashedInputPassword;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                hashedInputPassword = Convert.ToBase64String(hash);
            }

            using var conn = new MySqlConnection(_connection); //we open connection in the db
            conn.Open();

            string query = "SELECT Id, Username, Email FROM Users WHERE Email = @email AND PasswordHash = @password";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", hashedInputPassword);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userId = reader.GetInt32("Id");

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Username", reader.GetString("Username"));
             if (email == "admin@gmail.com")
            {
                return RedirectToAction("Admin", "Admin");
             }
            else
            {
                return RedirectToAction("Index", "Dashboard");
           }
                
            }

            ViewBag.Error = "Invalid email or password";
            return View(); // login failed we return view which is the login page itself
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}