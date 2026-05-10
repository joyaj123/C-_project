using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BusBookingSystem.Models;

namespace BusBookingSystem.Controllers
{
    public class AdminController : BaseController
    {
        private readonly string _connection;

        public AdminController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IActionResult? CheckLogin()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Login");

            return null;
        }

        public IActionResult Admin()
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            List<Trip> trips = new List<Trip>();

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = @"
                SELECT 
                    t.Id,
                    t.BusId,
                    t.RouteId,
                    b.Name AS BusName,
                    CONCAT(c1.Name, ' → ', c2.Name) AS RouteName,
                    t.DepartureTime,
                    t.Price
                FROM Trip t
                JOIN Bus b ON t.BusId = b.Id
                JOIN Route r ON t.RouteId = r.Id
                JOIN City c1 ON r.FromCityId = c1.Id
                JOIN City c2 ON r.ToCityId = c2.Id
                ORDER BY t.DepartureTime;
            ";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                trips.Add(new Trip
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    BusId = Convert.ToInt32(reader["BusId"]),
                    RouteId = Convert.ToInt32(reader["RouteId"]),
                    BusName = reader["BusName"].ToString(),
                    RouteName = reader["RouteName"].ToString(),
                    DepartureTime = Convert.ToDateTime(reader["DepartureTime"]),
                    Price = Convert.ToDecimal(reader["Price"])
                });
            }

            return View(trips);
        }

        public IActionResult Create()
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            ViewBag.Buses = GetBuses();
            ViewBag.Routes = GetRoutes();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Trip trip)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = @"
                INSERT INTO Trip (BusId, RouteId, DepartureTime, Price)
                VALUES (@BusId, @RouteId, @DepartureTime, @Price);
            ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@BusId", trip.BusId);
            cmd.Parameters.AddWithValue("@RouteId", trip.RouteId);
            cmd.Parameters.AddWithValue("@DepartureTime", trip.DepartureTime);
            cmd.Parameters.AddWithValue("@Price", trip.Price);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Admin");
        }

        public IActionResult Edit(int id)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            Trip trip = new Trip();

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = "SELECT * FROM Trip WHERE Id = @Id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                trip.Id = Convert.ToInt32(reader["Id"]);
                trip.BusId = Convert.ToInt32(reader["BusId"]);
                trip.RouteId = Convert.ToInt32(reader["RouteId"]);
                trip.DepartureTime = Convert.ToDateTime(reader["DepartureTime"]);
                trip.Price = Convert.ToDecimal(reader["Price"]);
            }

            ViewBag.Buses = GetBuses();
            ViewBag.Routes = GetRoutes();

            return View(trip);
        }

        [HttpPost]
        public IActionResult Edit(Trip trip)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = @"
                UPDATE Trip
                SET 
                    BusId = @BusId,
                    RouteId = @RouteId,
                    DepartureTime = @DepartureTime,
                    Price = @Price
                WHERE Id = @Id;
            ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", trip.Id);
            cmd.Parameters.AddWithValue("@BusId", trip.BusId);
            cmd.Parameters.AddWithValue("@RouteId", trip.RouteId);
            cmd.Parameters.AddWithValue("@DepartureTime", trip.DepartureTime);
            cmd.Parameters.AddWithValue("@Price", trip.Price);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Admin");
        }

        public IActionResult Delete(int id)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = "DELETE FROM Trip WHERE Id = @Id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Admin");
        }

        private List<dynamic> GetBuses()
        {
            var buses = new List<dynamic>();

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = "SELECT Id, Name FROM Bus ORDER BY Name";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                buses.Add(new
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString()
                });
            }

            return buses;
        }

        private List<dynamic> GetRoutes()
        {
            var routes = new List<dynamic>();

            using MySqlConnection conn = new MySqlConnection(_connection);
            conn.Open();

            string query = @"
                SELECT 
                    r.Id,
                    CONCAT(c1.Name, ' → ', c2.Name) AS RouteName
                FROM Route r
                JOIN City c1 ON r.FromCityId = c1.Id
                JOIN City c2 ON r.ToCityId = c2.Id
                ORDER BY c1.Name, c2.Name;
            ";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                routes.Add(new
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    RouteName = reader["RouteName"].ToString()
                });
            }

            return routes;
        }
    }
}