namespace FlightBookingWeb.ViewModels
{
    public class PaymentConfirmViewModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        // Thông tin chuyến bay
        public int OutboundFlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public List<string> OutboundSeats { get; set; }
        public List<string> ReturnSeats { get; set; }
    }
}
