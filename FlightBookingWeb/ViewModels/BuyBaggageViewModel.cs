namespace FlightBookingWeb.ViewModels
{
    public class BuyBaggageViewModel
    {
        public int TicketId { get; set; }
        public int ExtraBaggageKg { get; set; }
        public decimal Fee { get; set; }

        public int FlightId { get; set; }
        public string? DepartureAirport { get; set; }
        public string? ArrivalAirport { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public string? SeatNumber { get; set; }
    }

}