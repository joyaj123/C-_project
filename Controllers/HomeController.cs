using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BusBookingSystem.Controllers
{
    public class HomeController : BaseController
    {
        private readonly string _connection;

        public HomeController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpPost]
        public IActionResult ActivateTicket(int ticketId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            int userId = GetUserId();

            using var conn = new MySqlConnection(_connection);
            conn.Open();

            string getTicketQuery = @"
                SELECT Type, IsActive, ExpiresAt
                FROM Ticket
                WHERE Id = @Id AND UserId = @UserId";

            string ticketType = "";
            bool isActive = false;
            DateTime? expiresAt = null;

            using (var getCmd = new MySqlCommand(getTicketQuery, conn))
            {
                getCmd.Parameters.AddWithValue("@Id", ticketId);
                getCmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = getCmd.ExecuteReader();

                if (reader.Read())
                {
                    ticketType = reader.GetString("Type");
                    isActive = reader.GetBoolean("IsActive");

                    if (!reader.IsDBNull(reader.GetOrdinal("ExpiresAt")))
                        expiresAt = reader.GetDateTime("ExpiresAt");
                }
                else
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction("Index");
                }
            }

            if (isActive && expiresAt != null && DateTime.Now <= expiresAt.Value)
            {
                TempData["Error"] = "Ticket is already active.";
                return RedirectToAction("Index");
            }

            DateTime newExpiresAt;

            if (ticketType == "DAY")
                newExpiresAt = DateTime.Now.AddDays(1);
            else if (ticketType == "WEEK")
                newExpiresAt = DateTime.Now.AddDays(7);
            else
                newExpiresAt = DateTime.Now.AddMonths(1);

            string updateQuery = @"
                UPDATE Ticket
                SET 
                    IsActive = 1,
                    ActivatedAt = @ActivatedAt,
                    ExpiresAt = @ExpiresAt
                WHERE Id = @Id AND UserId = @UserId";

            using (var cmd = new MySqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@ActivatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@ExpiresAt", newExpiresAt);
                cmd.Parameters.AddWithValue("@Id", ticketId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    TempData["Success"] = "Ticket activated successfully!";
                else
                    TempData["Error"] = "Could not activate ticket.";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("Home")]
        public IActionResult Home(int? fromCityId, int? toCityId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            var trips = new List<object>();
            var cities = new List<object>();
            var tickets = new List<dynamic>();

            int userId = GetUserId();

            using var conn = new MySqlConnection(_connection);
            conn.Open();

            // -------------------------
            // LOAD CITIES
            // -------------------------
            string cityQuery = @"
                SELECT Id, Name
                FROM City
                ORDER BY Name ASC";

            using (var cityCmd = new MySqlCommand(cityQuery, conn))
            using (var cityReader = cityCmd.ExecuteReader())
            {
                while (cityReader.Read())
                {
                    cities.Add(new
                    {
                        Id = cityReader.GetInt32("Id"),
                        Name = cityReader.GetString("Name")
                    });
                }
            }

            // -------------------------
            // LOAD TRIPS
            // -------------------------
            string tripQuery = @"
                SELECT 
                    Trip.Id,
                    Bus.Name,
                    c1.Name AS FromCity,
                    c2.Name AS ToCity,
                    Trip.DepartureTime,
                    Trip.Price
                FROM Trip
                JOIN Bus ON Trip.BusId = Bus.Id
                JOIN Route ON Trip.RouteId = Route.Id
                JOIN City c1 ON Route.FromCityId = c1.Id
                JOIN City c2 ON Route.ToCityId = c2.Id
                WHERE (@FromCityId IS NULL OR Route.FromCityId = @FromCityId)
                  AND (@ToCityId IS NULL OR Route.ToCityId = @ToCityId)
                ORDER BY Trip.DepartureTime ASC";

            using (var cmd = new MySqlCommand(tripQuery, conn))
            {
                cmd.Parameters.AddWithValue("@FromCityId", (object?)fromCityId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToCityId", (object?)toCityId ?? DBNull.Value);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    trips.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        BusName = reader.GetString("Name"),
                        FromCity = reader.GetString("FromCity"),
                        ToCity = reader.GetString("ToCity"),
                        DepartureTime = reader.GetDateTime("DepartureTime"),
                        Price = reader.GetDecimal("Price")
                    });
                }
            }

            // -------------------------
            // LOAD ALL TICKETS (FIXED)
            // -------------------------
            string ticketQuery = @"
                SELECT 
                    Id,
                    Type,
                    Price,
                    IsActive,
                    ActivatedAt,
                    ExpiresAt
                FROM Ticket
                WHERE UserId = @UserId
                ORDER BY Id DESC";

            using (var cmd2 = new MySqlCommand(ticketQuery, conn))
            {
                cmd2.Parameters.AddWithValue("@UserId", userId);

                using var reader2 = cmd2.ExecuteReader();

                while (reader2.Read())
                {
                    tickets.Add(new
                    {
                        Id = reader2.GetInt32("Id"),
                        Type = reader2.GetString("Type"),
                        Price = reader2.GetDecimal("Price"),
                        IsActive = reader2.GetBoolean("IsActive"),
                        ActivatedAt = reader2.IsDBNull(reader2.GetOrdinal("ActivatedAt"))
                            ? (DateTime?)null
                            : reader2.GetDateTime("ActivatedAt"),
                        ExpiresAt = reader2.IsDBNull(reader2.GetOrdinal("ExpiresAt"))
                            ? (DateTime?)null
                            : reader2.GetDateTime("ExpiresAt")
                    });
                }
            }

            // -------------------------
            // CHECK IF USER CAN TRAVEL
            // -------------------------
            bool canTravel = tickets.Any(t =>
                t.IsActive &&
                (t.ExpiresAt == null || DateTime.Now <= t.ExpiresAt)
            );

            // -------------------------
            // VIEWBAG
            // -------------------------
            ViewBag.Tickets = tickets;
            ViewBag.CanTravel = canTravel;
            ViewBag.Cities = cities;
            ViewBag.SelectedFromCityId = fromCityId;
            ViewBag.SelectedToCityId = toCityId;

            return View(trips);
        }
    }
}