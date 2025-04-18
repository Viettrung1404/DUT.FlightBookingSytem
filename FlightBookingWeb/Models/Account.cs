using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Account
{
    public string AccountId { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool? Gender { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
