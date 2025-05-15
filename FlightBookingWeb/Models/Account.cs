using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;

    public bool? Gender { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<SeatBooking> SeatBookings { get; set; } = new List<SeatBooking>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
