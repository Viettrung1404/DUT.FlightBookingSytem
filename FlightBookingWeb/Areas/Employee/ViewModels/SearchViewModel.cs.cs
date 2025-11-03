using System.ComponentModel.DataAnnotations;

namespace FlightBookingWeb.ViewModels
{
    public class SearchViewModel
    {
        // Thong tin tai khoan

        public int AccountId { get; set; }

        [Required(ErrorMessage = "Ten dang nhap la bat buoc")]
        [StringLength(100, ErrorMessage = "Ten dang nhap toi da 100 ky tu")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email la bat buoc")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "So dien thoai khong hop le")]
        [StringLength(15, ErrorMessage = "So dien thoai toi da 15 ky tu")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vai tro la bat buoc")]
        [RegularExpression("^(Admin|Employee|User)$", ErrorMessage = "Vai tro khong hop le")]
        public string Role { get; set; } = null!;

        // Thong tin ve

        public int TicketId { get; set; }

        [StringLength(20, ErrorMessage = "Trang thai toi da 20 ky tu")]
        public string? Status { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngay dat ve")]
        public DateTime? BookingDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Gia ve phai lon hon hoac bang 0")]
        public decimal Price { get; set; }

        [Display(Name = "Ma chuyen bay")]
        public int FlightID { get; set; }
    }
}
