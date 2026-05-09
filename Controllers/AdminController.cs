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
            _connection = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        private IActionResult? CheckLogin()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Login");
            }

            return null;
        }

        public IActionResult Admin()
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            List<Trip> trips = new List<Trip>();

            using (MySqlConnection conn = new MySqlConnection(_connection))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        Id,
                        BusId,
                        RouteId,
                        DepartureTime,
                        Price
                    FROM Trip
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        trips.Add(new Trip
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            BusId = Convert.ToInt32(reader["BusId"]),
                            RouteId = Convert.ToInt32(reader["RouteId"]),
                            DepartureTime = Convert.ToDateTime(reader["DepartureTime"]),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }

            return View(trips);
        }
        public IActionResult Create()
{
    var auth = CheckLogin();
    if (auth != null)
        return auth;

    return View();
}
[HttpPost]
public IActionResult Create(Trip trip)
{
    var auth = CheckLogin();
    if (auth != null)
        return auth;

    using (MySqlConnection conn = new MySqlConnection(_connection))
    {
        conn.Open();

        string query = @"
            INSERT INTO Trip (BusId, RouteId, DepartureTime, Price)
            VALUES (@BusId, @RouteId, @DepartureTime, @Price)
        ";

        using var cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@BusId", trip.BusId);
        cmd.Parameters.AddWithValue("@RouteId", trip.RouteId);
        cmd.Parameters.AddWithValue("@DepartureTime", trip.DepartureTime);
        cmd.Parameters.AddWithValue("@Price", trip.Price);

        cmd.ExecuteNonQuery();
    }

    return RedirectToAction("Admin");
}
        public IActionResult Delete(int id)
{
    var auth = CheckLogin();

    if (auth != null)
        return auth;

    using (MySqlConnection conn = new MySqlConnection(_connection))
    {
        conn.Open();

        string query = "DELETE FROM Trip WHERE Id = @Id";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@Id", id);

        cmd.ExecuteNonQuery();
    }

    return RedirectToAction("Admin");
}
  public IActionResult Edit(int id)
{
    var auth = CheckLogin();

    if (auth != null)
        return auth;

    Trip trip = new Trip();

    using (MySqlConnection conn = new MySqlConnection(_connection))
    {
        conn.Open();

        string query = @"
            SELECT *
            FROM Trip
            WHERE Id = @Id
        ";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@Id", id);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                trip.Id = Convert.ToInt32(reader["Id"]);
                trip.BusId = Convert.ToInt32(reader["BusId"]);
                trip.RouteId = Convert.ToInt32(reader["RouteId"]);
                trip.DepartureTime = Convert.ToDateTime(reader["DepartureTime"]);
                trip.Price = Convert.ToDecimal(reader["Price"]);
            }
        }
    }

    return View(trip);
}
    [HttpPost]
public IActionResult Edit(Trip trip)
{
    var auth = CheckLogin();

    if (auth != null)
        return auth;

    using (MySqlConnection conn = new MySqlConnection(_connection))
    {
        conn.Open();

        string query = @"
            UPDATE Trip
            SET
                BusId = @BusId,
                RouteId = @RouteId,
                DepartureTime = @DepartureTime,
                Price = @Price
            WHERE Id = @Id
        ";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@Id", trip.Id);
        cmd.Parameters.AddWithValue("@BusId", trip.BusId);
        cmd.Parameters.AddWithValue("@RouteId", trip.RouteId);
        cmd.Parameters.AddWithValue("@DepartureTime", trip.DepartureTime);
        cmd.Parameters.AddWithValue("@Price", trip.Price);

        cmd.ExecuteNonQuery();
    }

    return RedirectToAction("Admin");
}
    }
}