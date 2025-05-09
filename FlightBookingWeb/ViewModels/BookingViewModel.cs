using System.ComponentModel.DataAnnotations;
namespace FlightBookingWeb.ViewModels
{
    public class BookingViewModel
    {
        [Required(ErrorMessage = "Flight ID is required")]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Passenger name is required")]
        [StringLength(100, ErrorMessage = "Name must be between 3 and 100 characters", MinimumLength = 3)]
        public string PassengerName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Seat position is required")]
        public string SeatPosition { get; set; }

        [Required(ErrorMessage = "Class is required")]
        public string TicketClass { get; set; } // Economy, Business, etc.

        public decimal Price { get; set; } // Giá vé
    }
}

