namespace BusBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int TripId { get; set; }

        public DateTime BookingTime { get; set; }
    }
}