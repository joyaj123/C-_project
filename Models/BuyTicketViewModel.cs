using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models
{
    public class BuyTicketViewModel
    {
        [Required(ErrorMessage = "Please select a ticket duration.")]
        public string Duration { get; set; } = "";

        [Required(ErrorMessage = "Please enter payer name.")]
        public string PayerName { get; set; } = "";

        [Required(ErrorMessage = "Please enter phone number.")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "";

        public string? CardLast4 { get; set; }
    }
}