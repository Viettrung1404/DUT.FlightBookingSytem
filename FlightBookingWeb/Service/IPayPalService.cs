namespace FlightBookingWeb.Service
{
    public interface IPayPalService
    {
        Task<string> CreateOrderAsync(decimal amount, string currency, string description);
        Task<bool> CapturePaymentAsync(string orderId);
    }
}
