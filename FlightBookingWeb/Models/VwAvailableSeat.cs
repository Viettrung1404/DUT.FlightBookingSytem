using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class VwAvailableSeat
{
    public int FlightId { get; set; }

    public int SeatId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string SeatClass { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public decimal? Price { get; set; }
}
