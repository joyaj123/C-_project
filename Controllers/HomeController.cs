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

        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            var trips = new List<object>();
            object? ticket = null;

            int userId = GetUserId();

            using var conn = new MySqlConnection(_connection);
            conn.Open();

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
                ORDER BY Trip.DepartureTime ASC";

            using (var cmd = new MySqlCommand(tripQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    trips.Add(new
                    {
                        Id = reader.GetInt32(0),
                        BusName = reader.GetString(1),
                        FromCity = reader.GetString(2),
                        ToCity = reader.GetString(3),
                        DepartureTime = reader.GetDateTime(4),
                        Price = reader.GetDecimal(5)
                    });
                }
            }

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
                ORDER BY Id DESC
                LIMIT 1";

            using (var cmd2 = new MySqlCommand(ticketQuery, conn))
            {
                cmd2.Parameters.AddWithValue("@UserId", userId);

                using var reader2 = cmd2.ExecuteReader();

                if (reader2.Read())
                {
                    ticket = new
                    {
                        Id = reader2.GetInt32(0),
                        Type = reader2.GetString(1),
                        Price = reader2.GetDecimal(2),
                        IsActive = reader2.GetBoolean(3),
                        ActivatedAt = reader2.IsDBNull(4) ? (DateTime?)null : reader2.GetDateTime(4),
                        ExpiresAt = reader2.IsDBNull(5) ? (DateTime?)null : reader2.GetDateTime(5)
                    };
                }
            }

            ViewBag.Ticket = ticket;

            return View(trips);
        }
    }
}