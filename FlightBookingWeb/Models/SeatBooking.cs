using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class SeatBooking
{
    public int SeatBookingId { get; set; }

    public int FlightId { get; set; }

    public int SeatId { get; set; }

    public bool IsBooked { get; set; }

    public DateTime? BookingDate { get; set; }

    public int? AccountId { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Flight Flight { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;
}
