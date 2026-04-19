using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BusBookingSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace BusBookingSystem.Controllers
{
    public class RegisterController : Controller
    {
        private readonly string _connection;

        public RegisterController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }
       
        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < 8)
                return false;

            bool hasLower = false;
            bool hasUpper = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }

            return hasLower && hasUpper && hasDigit && hasSpecial;
        }

        // GET: /Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(user.Username) ||
                    string.IsNullOrEmpty(user.Email) ||
                    string.IsNullOrEmpty(user.Password))
                {
                    ViewBag.Error = "All fields are required";
                    return View(user);
                }

                // Check if email exists
                using (MySqlConnection conn = new MySqlConnection(_connection))
                {
                    conn.Open();

                    string checkEmail = "SELECT COUNT(*) FROM Users WHERE Email=@e";

                    using (var checkCmd = new MySqlCommand(checkEmail, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@e", user.Email);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            ViewBag.Error = "Email already exists";
                            return View(user);
                        }
                    }
                }

                // Password strength check
                if (!IsStrongPassword(user.Password))
                {
                    ViewBag.Error = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.";
                    return View(user);
                }

                // Hash password
                string hashedPassword;
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(user.Password);
                    var hash = sha256.ComputeHash(bytes);
                    hashedPassword = Convert.ToBase64String(hash);
                }

                // Insert user
                using (MySqlConnection conn = new MySqlConnection(_connection))
                {
                    conn.Open();

                    string insertQuery = "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@u, @e, @p)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", user.Username);
                        cmd.Parameters.AddWithValue("@e", user.Email);
                        cmd.Parameters.AddWithValue("@p", hashedPassword);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Success - redirect to login
                return RedirectToAction("Login", "LogIn");///////////////////////////////
            }
            catch (Exception ex)
            {
                // Any database or other error - show friendly message
                ViewBag.Error = "Registration failed. Please try again later.";
                // Optional: log the error
                Console.WriteLine($"Registration error: {ex.Message}");
                return View(user);
            }
        }
    }
}




/*if (HttpContext.Session.GetString("Role") != "Admin")
{
    return RedirectToAction("Index", "Home");
}*/