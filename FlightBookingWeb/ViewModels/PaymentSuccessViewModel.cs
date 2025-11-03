namespace FlightBookingWeb.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal AmountPaid { get; set; }

        // Thông tin chuyến bay
        public int FlightId { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureDate { get; set; }

        // Thông tin hành khách
        public List<string> PassengerNames { get; set; } = new();
        public List<string> SeatNumbers { get; set; } = new();

        // PayPal-specific
        public string? PayPalPayerId { get; set; }
        public string? PayPalPaymentId { get; set; }
    }
}