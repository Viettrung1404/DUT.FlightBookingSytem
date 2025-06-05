using Microsoft.AspNetCore.Mvc;

namespace FlightBookingWeb.Service
{
    public static class SeatPricingHelper
    {
        public static decimal CalculateSeatPrice(decimal basePrice, string seatClass, string seatType)
        {
            decimal price = basePrice;

            // Điều chỉnh theo hạng ghế
            switch (seatClass)
            {
                case "Business":
                    price *= 1.5m; // +50%
                    break;
                case "Economy":
                default:
                    // Không thay đổi
                    break;
            }

            // Điều chỉnh theo loại ghế
            switch (seatType)
            {
                case "Window":
                    price *= 1.1m; // +10%
                    break;
                case "Aisle":
                    price *= 1.05m; // +5%
                    break;
                case "Middle":
                default:
                    // Không thay đổi
                    break;
            }
            return Math.Round(price, 2);
        }

    }
}
