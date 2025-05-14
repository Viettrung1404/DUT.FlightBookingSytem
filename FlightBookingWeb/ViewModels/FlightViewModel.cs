using System.ComponentModel.DataAnnotations;

namespace FlightBookingWeb.ViewModels
{
    public class FlightViewModel
    {
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Departure airport is required.")]
        [StringLength(100, ErrorMessage = "Departure airport cannot exceed 100 characters.")]
        public string DepartureAirport { get; set; } = null!;

        [Required(ErrorMessage = "Arrival airport is required.")]
        [StringLength(100, ErrorMessage = "Arrival airport cannot exceed 100 characters.")]
        public string ArrivalAirport { get; set; } = null!;

        [Required(ErrorMessage = "Departure date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [Display(Name = "Departure Date")]
        public DateTime DepartureOutBoardDate { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [Display(Name = "Return Date")]
        [CompareDates("DepartureOutBoardDate", ErrorMessage = "Return date must be after the departure date.")]
        public DateTime? DepartureReturnDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal? Price { get; set; }

        public bool IsRoundTrip { get; set; } 

        [Required(ErrorMessage = "Passenger count is required.")]
        [Range(1, 10, ErrorMessage = "Passenger count must be between 1 and 10.")]
        public int PassengerCount { get; set; } = 1;

        public List<string> Cities { get; set; } = new();
    }

    // Custom validation attribute for comparing dates
    public class CompareDatesAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public CompareDatesAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = value as DateTime?;
            var comparisonValue = validationContext.ObjectType.GetProperty(_comparisonProperty)?.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
