namespace FlightBookingWeb.ViewModels
{
    public class SeatSelectionViewModel
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; }
        public string SeatClass { get; set; }
        public string SeatType { get; set; }
        public object? IsBooked { get; set; }
        public int OutboundFlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public int PassengerCount { get; set; }
        public List<SeatViewModel>? AvailableReturnSeats { get; set; }
        public required List<SeatViewModel> AvailableOutboundSeats { get; set; }  
        public bool IsRoundTrip { get; set; }
        public List<string> SelectedSeatsOutBoard { get; set; } = new List<string>();
        public List<string>? SelectedSeatsReturnBoard { get; set; } = new List<string>();
    }
}
