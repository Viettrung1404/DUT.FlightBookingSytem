namespace FlightBookingWeb.ViewModels
{
    public class SearchViewModel
    {
        // Thong tin tai khoan
        public int AccountId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;

        // Thong tin ve
        public int TicketId { get; set; }
        public string? Status { get; set; }
        public DateTime? BookingDate { get; set; }
        public decimal Price { get; set; }
        public string FlightName { get; set; } = null!;
    }
}
