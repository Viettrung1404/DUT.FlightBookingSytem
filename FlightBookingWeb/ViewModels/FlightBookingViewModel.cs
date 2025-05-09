using System.Collections.Generic;

namespace FlightBookingWeb.ViewModels
{
    public class FlightBookingViewModel
    {
        public int FlightId { get; set; }
        public int NumberOfTickets { get; set; } // Số lượng vé
        public string TicketClass { get; set; } // Hạng vé (Economy, Business, First Class)
        public List<PassengerInfo> Passengers { get; set; } = new List<PassengerInfo>();
    }

    public class PassengerInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
