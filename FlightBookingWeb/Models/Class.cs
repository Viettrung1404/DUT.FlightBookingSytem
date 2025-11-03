using System;

namespace FlightBookingWeb.Models
{
    public partial class VwAvailableSeat
    {
        public int FlightId { get; set; } // ID của chuyến bay
        public int SeatId { get; set; } // ID của ghế
        public string SeatNumber { get; set; } // Số ghế
        public string SeatClass { get; set; } // Hạng ghế (Economy, Business, First Class)
        public string SeatType { get; set; } // Loại ghế (Window, Aisle, Middle)
        public decimal? Price { get; set; } // Giá vé
    }
}

