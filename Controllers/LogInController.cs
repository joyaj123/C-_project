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

        // Constructor - reads connection string from appsettings.json
        public LogInController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET: /LogIn/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /LogIn/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            // Hash the input password to compare with stored hash
            string hashedInputPassword;
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                hashedInputPassword = Convert.ToBase64String(hash);
            }

            using (MySqlConnection conn = new MySqlConnection(_connection))
            {
                conn.Open();

                string query = "SELECT Id, Username, Email, PasswordHash FROM Users WHERE Email = @email AND PasswordHash = @password";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", hashedInputPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Login successful
                            var user = new User
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email")
                            };
                            
                            // Store user info in session
                            HttpContext.Session.SetString("UserId", user.Id.ToString());
                            HttpContext.Session.SetString("Username", user.Username);
                            HttpContext.Session.SetString("Email", user.Email);
                            
                            // Redirect to home page after successful login
                            return RedirectToAction("Index", "Home");///////////////////
                        }
                        else
                        {
                            ViewBag.Error = "Invalid email or password";
                            return View();
                        }
                    }
                }
            }
        }

        // Logout action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}