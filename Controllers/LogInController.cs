using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BusBookingSystem.Controllers
{
    public class LogInController : Controller
    {
        private string connStr = "server=localhost;database=bus_booking_db;user=root;password=1234;";

        // SHOW LOGIN PAGE
        public IActionResult Login()
        {
            return View();
        }

        // HANDLE LOGIN HON BI BALECH AL SITE 
        [HttpPost]
        public IActionResult Login(string email, string password) //name of the route
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT Id, Username FROM Users WHERE Email=@Email AND PasswordHash=@Password";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", password);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                HttpContext.Session.SetInt32("UserId", reader.GetInt32(0));
                HttpContext.Session.SetString("Username", reader.GetString(1));

                return RedirectToAction("Index", "Home");////////////
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}