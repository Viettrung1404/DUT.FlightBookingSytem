using Microsoft.AspNetCore.Mvc;

namespace FlightBookingWeb.ViewModels
{
    public class FlightViewModel
    {
        public int FlightId { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureOutBoardDate { get; set; }
        public DateTime? DepartureReturnDate { get; set; }
        //public DateTime ArrivalDate { get; set; }
        public decimal? Price { get; set; }
        public bool IsRoundTrip { get; set; } = true;
        public int PassengerCount { get;  set; } = 1;
        //public object? ReturnDate { get; internal set; }
    }
}
