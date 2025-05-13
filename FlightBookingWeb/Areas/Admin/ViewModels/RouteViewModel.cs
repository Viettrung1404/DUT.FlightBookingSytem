using System.ComponentModel.DataAnnotations;

namespace FlightBookingWeb.ViewModels
{
    public class RouteViewModel : IValidatableObject
    {
        public int RouteId { get; set; }

        [Required(ErrorMessage = "San bay di la bat buoc")]
        public int DepartureAirportId { get; set; }

        [Required(ErrorMessage = "San bay den la bat buoc")]
        public int ArrivalAirportId { get; set; }

        public string DepartureAirportName { get; set; } = string.Empty;

        public string ArrivalAirportName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thoi gian bay la bat buoc")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thoi gian phai dung dinh dang HH:mm")]
        public string Duration { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Gia co ban phai lon hon 0")]
        public decimal BasePrice { get; set; }

        // ✅ Validate thủ công: không cho phép sân bay đi trùng sân bay đến
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DepartureAirportId == ArrivalAirportId)
            {
                yield return new ValidationResult(
                    "San bay di va den phai khac nhau.",
                    new[] { nameof(DepartureAirportId), nameof(ArrivalAirportId) });
            }
        }
    }
}
