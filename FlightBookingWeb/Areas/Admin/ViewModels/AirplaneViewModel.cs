using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;

namespace FlightBookingWeb.Areas.Admin.ViewModels
{
    public class AirplaneViewModel : IValidatableObject
    {
        public int AirplaneId { get; set; }

        [Required(ErrorMessage = "Tên máy bay là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        [DisplayName("Tên máy bay")]
        public string AirplaneName { get; set; } = null!;

        [Required(ErrorMessage = "Loại máy bay là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại máy bay không được vượt quá 50 ký tự")]
        [DisplayName("Loại máy bay")]
        public string AirplaneType { get; set; } = null!;

        [Required(ErrorMessage = "Tổng ghế là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Tổng ghế phải từ {1} đến {2}")]
        [DisplayName("Tổng ghế")]
        public int TotalSeats { get; set; }

        [Required(ErrorMessage = "Ghế phổ thông là bắt buộc")]
        [Range(0, 1000, ErrorMessage = "Ghế phổ thông phải từ {1} đến {2}")]
        [DisplayName("Ghế phổ thông")]
        public int EconomySeats { get; set; }

        [Required(ErrorMessage = "Ghế thương gia là bắt buộc")]
        [Range(0, 1000, ErrorMessage = "Ghế thương gia phải từ {1} đến {2}")]
        [DisplayName("Ghế thương gia")]
        public int BusinessSeats { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
        [DisplayName("Năm sản xuất")]
        public int? ManufactureYear { get; set; }

        [StringLength(20, ErrorMessage = "Trạng thái không quá 20 ký tự")]
        [RegularExpression("^(Active|Deleted)$", ErrorMessage = "Trạng thái không hợp lệ")]
        [DisplayName("Trạng thái")]
        public string? Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate tổng ghế = Economy + Business
            if (EconomySeats + BusinessSeats != TotalSeats)
            {
                yield return new ValidationResult(
                    "Tổng ghế phải bằng Ghế phổ thông + Ghế thương gia",
                    new[] { nameof(TotalSeats), nameof(EconomySeats), nameof(BusinessSeats) }
                );
            }

            // Validate năm sản xuất không vượt quá năm hiện tại
            if (ManufactureYear.HasValue && ManufactureYear > DateTime.Now.Year)
            {
                yield return new ValidationResult(
                    "Năm sản xuất không được lớn hơn năm hiện tại",
                    new[] { nameof(ManufactureYear) }
                );
            }
        }
    }
}