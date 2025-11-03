
namespace FlightBookingWeb.ViewModels
{
    public class FlightSearchResultsViewModel
    {
        public List<FlightViewModel> OutboundFlights { get; set; }
        public List<FlightViewModel>? ReturnFlights { get; set; }
        public bool IsRoundTrip { get; internal set; }
        public object PassengerCount { get; internal set; }
    }
}
