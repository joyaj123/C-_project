namespace BusBookingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } // Card / Cash / Online

        public string Status { get; set; } = ""; // SUCCESS, FAILED

        public DateTime PaidAt { get; set; }
    }
}