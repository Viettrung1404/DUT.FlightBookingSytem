using FlightBookingWeb.Service;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System.Net;

namespace FlightBookingWeb.Service
{

    public class PayPalService : IPayPalService
    {
        private readonly PayPalHttpClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<PayPalService> _logger;

        public PayPalService(IConfiguration config, ILogger<PayPalService> logger)
        {
            _config = config;
            _logger = logger;

            // Authentication
            var environment = new SandboxEnvironment(
                _config["PayPal:ClientId"],
                _config["PayPal:Secret"]);

            _client = new PayPalHttpClient(environment);

        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency, string description)
        {
            try
            {
                var order = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = _config["PayPal:ReturnUrl"],
                        CancelUrl = _config["PayPal:CancelUrl"],
                        BrandName = _config["PayPal:BrandName"],
                        UserAction = "PAY_NOW"
                    },
                    PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    Description = description,
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = currency,
                        Value = amount.ToString("0.00"),
                        AmountBreakdown = new AmountBreakdown
                        {
                            ItemTotal = new Money
                            {
                                CurrencyCode = currency,
                                Value = amount.ToString("0.00")
                            }
                        }
                    }
                }
            }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(order);

                var response = await _client.Execute(request);
                var result = response.Result<Order>();

                _logger.LogInformation($"PayPal order created: {result.Id}");
                return result.Id; // Trả về orderId cho PayPal JS SDK
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal order");
                throw;
            }
        }


        public async Task<bool> CapturePaymentAsync(string orderId)
        {
            try
            {
                var request = new OrdersCaptureRequest(orderId);
                request.RequestBody(new OrderActionRequest());
                var response = await _client.Execute(request);

                if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"PayPal capture failed: {response.StatusCode}");
                    return false;
                }

                var result = response.Result<Order>();
                _logger.LogInformation($"PayPal payment captured: {result.Id}");
                return result.Status == "COMPLETED";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing PayPal payment");
                return false;
            }
        }

    }
}