namespace FlightBookingWeb.ViewModels
{
    public class PayPalOrderRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; }
        public string? CustomId { get; set; } // FlightId
    }
}