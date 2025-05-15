namespace FlightBookingWeb.ViewModels
{
    public class SeatViewModel
    {
        public int SeatId { get; internal set; }
        public string SeatNumber { get; internal set; }
        public string SeatClass { get; internal set; }
        public string SeatType { get; internal set; }
        public bool? IsBooked { get; internal set; }
    }
}
