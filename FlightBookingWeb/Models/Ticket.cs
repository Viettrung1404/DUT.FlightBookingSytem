using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string? SeatPosition { get; set; }

    public int? Price { get; set; }

    public int? FlightId { get; set; }

    public int? CustomerId { get; set; }

    public int? ClassId { get; set; }

    public int? TransitAirportId { get; set; }

    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();

    public virtual TicketClass? Class { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Flight? Flight { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Airport? TransitAirport { get; set; }
}
