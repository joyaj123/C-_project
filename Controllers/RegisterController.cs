using Microsoft.AspNetCore.Mvc; //imports
using MySql.Data.MySqlClient;
using BusBookingSystem.Models; //import my models
using System.Security.Cryptography;
using System.Text;

namespace BusBookingSystem.Controllers //byenteme la hal folder 
{
    public class RegisterController : Controller //inhertis the controller functions
    {
        private readonly string _connection; //to get the string privatly

        public RegisterController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!; //connect to database form appsettings.json
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
        public IActionResult Register() //when the user open this is the first page li ha tbayen which is register  
        {
            return View();
        }

        // POST: /Register
        [HttpPost]
        public IActionResult Register(User user) //what happen when the user register 
        //user user for model binding so the input in html are connected to the html 
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(user.Username) ||
                    string.IsNullOrEmpty(user.Email) ||
                    string.IsNullOrEmpty(user.Password))
                {
                    ViewBag.Error = "All fields are required"; //to send the error to the view 
                    return View(user); //return the page with the same input 
                }

                // Check if email exists
                using (MySqlConnection conn = new MySqlConnection(_connection)) //open a connection to the database
                {
                    conn.Open();

                    string checkEmail = "SELECT COUNT(*) FROM Users WHERE Email=@e";

                    using (var checkCmd = new MySqlCommand(checkEmail, conn)) //create a  the query 
                    {
                        checkCmd.Parameters.AddWithValue("@e", user.Email);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar()); //return one number execute scalar

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
                    var hash = sha256.ComputeHash(bytes); //used sha256 for hashing 
                    hashedPassword = Convert.ToBase64String(hash); //convert to string to save in db 
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

                        cmd.ExecuteNonQuery(); //execute query that doesnt return data
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