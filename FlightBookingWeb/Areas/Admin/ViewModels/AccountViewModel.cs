using System.ComponentModel.DataAnnotations;

namespace FlightBookingWeb.Areas.Admin.ViewModels
{
    public class AccountViewModel
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = null!;

        public bool? Gender { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters.")]
        public string Role { get; set; } = null!;
    }
}
