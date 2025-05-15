using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public int AirplaneId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string SeatClass { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public bool? IsEmergencyExit { get; set; }

    public virtual Airplane Airplane { get; set; } = null!;

    public virtual ICollection<SeatBooking> SeatBookings { get; set; } = new List<SeatBooking>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
