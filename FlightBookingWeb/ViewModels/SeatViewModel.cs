namespace FlightBookingWeb.ViewModels
{
    public class SeatViewModel
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; } // "Available" hoặc "Booked"
        public bool IsBooked { get; set; }
    }
}
