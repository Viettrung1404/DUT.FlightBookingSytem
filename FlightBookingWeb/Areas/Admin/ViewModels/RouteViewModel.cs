using System.ComponentModel.DataAnnotations;

namespace FlightBookingWeb.ViewModels
{
    public class RouteViewModel
    {
        public int RouteId { get; set; }

        [Required(ErrorMessage = "San bay di la bat buoc")]
        public int DepartureAirportId { get; set; }

        public string ArrivalAirportName { get; set; } = string.Empty;

        [Required(ErrorMessage = "San bay den la bat buoc")]
        public int ArrivalAirportId { get; set; }

        public string DepartureAirportName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thoi gian bay la bat buoc")]
        public string Duration { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Gia co ban phai lon hon 0")]
        public decimal BasePrice { get; set; }
    }

}