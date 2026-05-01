using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BusBookingSystem.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly string _connection;

        public ProfileController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        public IActionResult Prof()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            int userId = GetUserId();

            User? user = null;
             List<Ticket> tickets = new List<Ticket>();

            using (var conn = new MySqlConnection(_connection))
            {
                await conn.OpenAsync();

                //Get user
                string userQuery = "SELECT Id, Username, Email, PasswordHash FROM Users WHERE Id = @id";

                using (var cmd = new MySqlCommand(userQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"))
                            };
                        }
                    }
                }

                if (user == null)
                    return NotFound();

                //Get tickets
                string ticketQuery = @"
                    SELECT Id, Type,Price, IsActive,ActivatedAt, ExpiresAt
                    FROM Ticket
                    WHERE UserId = @id";

                using (var cmd = new MySqlCommand(ticketQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tickets.Add(new Ticket
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Type=reader.GetString(reader.GetOrdinal("Type")),
                                Price=reader.GetDecimal(reader.GetOrdinal("Price")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                ActivatedAt=reader.IsDBNull(reader.GetOrdinal("ActivatedAt"))
                                ? null
                                :reader.GetDateTime(reader.GetOrdinal("ActivatedAt")),
                                ExpiresAt = reader.IsDBNull(reader.GetOrdinal("ExpiresAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
                            });
                        }
                    }
                }
            }

            
    var activeTickets = tickets
    .Where(t => t.IsActive && t.ExpiresAt > DateTime.UtcNow)
    .ToList();

    var inactiveTickets = tickets
    .Where(t => !t.IsActive || t.ExpiresAt <= DateTime.UtcNow)
    .ToList();

    
    ViewBag.ActiveTickets = activeTickets;
    ViewBag.InactiveTickets = inactiveTickets;


    ViewBag.HasActiveTicket = activeTickets.Any();
    ViewBag.Expiry = activeTickets.FirstOrDefault()?.ExpiresAt;

            return View(user);
        }
    }
}