namespace FlightBookingWeb.ViewModels
{
    public class FlightSearchViewModel
    {
        public string DepartureAirport { get; set; } // Điểm đi
        public string ArrivalAirport { get; set; } // Điểm đến
        public DateTime? DepartureDate { get; set; } // Ngày đi
    }
}
