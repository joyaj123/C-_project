namespace BusBookingSystem.Models
{
    public class Trip
    {
        public int Id { get; set; }

        public int BusId { get; set; }

        public int RouteId { get; set; }

        public string? BusName { get; set; }

        public string? RouteName { get; set; }

        public DateTime DepartureTime { get; set; }

        public decimal Price { get; set; }
    }
}