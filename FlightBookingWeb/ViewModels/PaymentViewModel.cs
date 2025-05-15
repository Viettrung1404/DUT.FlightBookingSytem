namespace FlightBookingWeb.ViewModels
{
    public class PaymentViewModel
    {
        public int OutboundFlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public bool IsRoundTrip { get; set; }
        public int PassengerCount { get; set; }

        public List<string> SelectedOutboundSeats { get; set; } = new List<string>();
        public List<string> SelectedReturnSeats { get; set; } = new List<string>();

        // Thông tin thanh toán
        public string PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }

        // Thông tin hành khách
        public List<PassengerInfo> Passengers { get; set; } = new List<PassengerInfo>();

        // Tổng tiền
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal => TotalAmount + TaxAmount;
    }

    public class PassengerInfo
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string PassportNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string CCCD { get; set; }
    }
}
