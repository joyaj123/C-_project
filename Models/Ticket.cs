using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BusBookingSystem.Models
{

public class Ticket
{
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Type { get; set; } = ""; // DAY, WEEK, MONTH

        public decimal Price { get; set; }

        public DateTime PurchasedAt { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }
}
}