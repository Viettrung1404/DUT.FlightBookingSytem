namespace FlightBookingWeb.ViewModels
{
    public class SeatConfirmViewModel
    {
        public int OutboundFlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public int PassengerCount { get; set; }
        public bool IsRoundTrip { get; set; }

        public List<SeatViewModel> SelectedOutboundSeats { get; set; } = new();
        public List<SeatViewModel> SelectedReturnSeats { get; set; } = new();
    }
}
