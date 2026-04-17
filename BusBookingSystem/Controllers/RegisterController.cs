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
            if (string.IsNullOrEmpty(user.Username) ||
                string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.Password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

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
            if (!IsStrongPassword(user.Password))
           {
             ViewBag.Error = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.";
             return View(user);
            }

            string hashedPassword;
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(user.Password);
                var hash = sha256.ComputeHash(bytes);
                hashedPassword = Convert.ToBase64String(hash);
            }

            using (MySqlConnection conn = new MySqlConnection(_connection))
            {
                conn.Open();

                string insertQuery =
                    "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@u, @e, @p)";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@u", user.Username);
                    cmd.Parameters.AddWithValue("@e", user.Email);
                    cmd.Parameters.AddWithValue("@p", hashedPassword);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Login");
        }
    }
}