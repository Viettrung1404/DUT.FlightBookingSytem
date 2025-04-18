using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Ticket
{
    public string TicketId { get; set; } = null!;

    public string? SeatPosition { get; set; }

    public int? Price { get; set; }

    public string? FlightId { get; set; }

    public string? CustomerId { get; set; }

    public string? ClassId { get; set; }

    public string? TransitAirportId { get; set; }

    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();

    public virtual TicketClass? Class { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Flight? Flight { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Airport? TransitAirport { get; set; }
}
