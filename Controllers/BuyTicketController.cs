using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BusBookingSystem.Models;

namespace BusBookingSystem.Controllers
{
    public class BuyTicketController : BaseController
    {
        private readonly string _connection;

        public BuyTicketController(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET
        public IActionResult Buy()
        {
            if (!IsLoggedIn()) //check if the user is logged in 
                return RedirectToAction("Login", "LogIn");

            return View(new BuyTicketViewModel());
        }

        [HttpPost]
        public IActionResult Buy(BuyTicketViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "LogIn");

            // Extra validation for card
            if (model.PaymentMethod == "CARD")
            {
                if (string.IsNullOrWhiteSpace(model.CardLast4))
                {
                    ModelState.AddModelError("CardLast4", "Please enter the last 4 digits of the card.");
                }
                else if (model.CardLast4.Length != 4 || !model.CardLast4.All(char.IsDigit))
                {
                    ModelState.AddModelError("CardLast4", "Card last 4 digits must be exactly 4 numbers.");
                }
            }

            // STOP here if form is invalid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int userId = GetUserId();

                string type;
                decimal price;
                DateTime expiresAt;

                if (model.Duration == "1day")
                {
                    type = "DAY";
                    price = 5;
                    expiresAt = DateTime.Now.AddDays(1);
                }
                else if (model.Duration == "1week")
                {
                    type = "WEEK";
                    price = 20;
                    expiresAt = DateTime.Now.AddDays(7);
                }
                else if (model.Duration == "1month")
                {
                    type = "MONTH";
                    price = 60;
                    expiresAt = DateTime.Now.AddMonths(1);
                }
                else
                {
                    ModelState.AddModelError("Duration", "Invalid ticket duration.");
                    return View(model);
                }

                using var conn = new MySqlConnection(_connection);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    string ticketQuery = @"
                        INSERT INTO Ticket
                        (UserId, Type, Price, IsActive, ActivatedAt, ExpiresAt)
                        VALUES
                        (@UserId, @Type, @Price, @IsActive, NULL, @ExpiresAt);
                        SELECT LAST_INSERT_ID();";

                    int ticketId;

                    using (var cmd = new MySqlCommand(ticketQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@IsActive", false);
                        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);

                        ticketId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    string paymentQuery = @"
                        INSERT INTO Payment
                        (TicketId, Amount, Method, Status, PayerName, PhoneNumber, CardLast4)
                        VALUES
                        (@TicketId, @Amount, @Method, @Status, @PayerName, @PhoneNumber, @CardLast4)";

                    using (var paymentCmd = new MySqlCommand(paymentQuery, conn, transaction))
                    {
                        paymentCmd.Parameters.AddWithValue("@TicketId", ticketId);
                        paymentCmd.Parameters.AddWithValue("@Amount", price);
                        paymentCmd.Parameters.AddWithValue("@Method", model.PaymentMethod);
                        paymentCmd.Parameters.AddWithValue("@Status", "SUCCESS");
                        paymentCmd.Parameters.AddWithValue("@PayerName", model.PayerName);
                        paymentCmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                        paymentCmd.Parameters.AddWithValue("@CardLast4",
                            string.IsNullOrWhiteSpace(model.CardLast4) ? DBNull.Value : model.CardLast4);

                        paymentCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Ticket purchase failed. Nothing was saved.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("", "Something went wrong while buying the ticket.");
                return View(model);
            }
        }
    }
}