using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using FlightBookingWeb.Models;

namespace FlightBookingWeb.ViewModels
{
    public class CheckoutViewModel
    {
        // Flight information
        [Required(ErrorMessage = "Thông tin chuyến bay đi là bắt buộc")]
        public Flight OutboundFlight { get; set; }

        public Flight? ReturnFlight { get; set; }

        // Seat selection
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một ghế đi")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một ghế đi")]
        public List<string> SelectedOutboundSeats { get; set; } = new List<string>();

        [RequiredIf(nameof(ReturnFlight), ErrorMessage = "Vui lòng chọn ghế về")]
        [SeatNumberValidation]
        public List<string>? SelectedReturnSeats { get; set; } = new List<string>();

        // Pricing
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền không hợp lệ")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        // Passenger info
        [Required(ErrorMessage = "Số lượng hành khách là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số lượng hành khách phải từ 1 đến 10")]
        public int PassengerCount { get; set; } = 1;

        // Payment
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        [CreditCard(ErrorMessage = "Số thẻ không hợp lệ")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Số thẻ phải từ 13-19 ký tự")]
        public string? CardNumber { get; set; }

        // Custom validation properties
        public bool IsRoundTrip => ReturnFlight != null;

        // Navigation properties
        [NotMapped]
        public List<string> AllPaymentMethods => Enum.GetNames(typeof(PaymentMethod)).ToList();

        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public string? ReturnDepartureCity { get; set; }
        public string? ReturnArrivalCity { get; set; }
        public List<PassengerInfo> Passengers { get; set; } = new List<PassengerInfo>();
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        PayPal,
        Cash
    }

    // Custom validation attribute
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public RequiredIfAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(_propertyName)?.GetValue(instance);

            if (propertyValue != null && (value == null || (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    // Custom seat validation
    public class SeatNumberValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is List<string> seats)
            {
                foreach (var seat in seats)
                {
                    if (!Regex.IsMatch(seat, @"^[A-Z]\d{1,2}$"))
                    {
                        return new ValidationResult($"Số ghế {seat} không hợp lệ (ví dụ: A1, B12)");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}