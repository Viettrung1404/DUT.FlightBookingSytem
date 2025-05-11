
namespace FlightBookingWeb.ViewModels
{
    public class FlightSearchResultsViewModel
    {
        public List<FlightViewModel> OutboundFlights { get; internal set; }
        public List<FlightViewModel>? ReturnFlights { get; internal set; }
        public bool IsRoundTrip { get; internal set; }
        public object PassengerCount { get; internal set; }
    }
}
