using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Baggage
{
    public string BaggageId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? TicketId { get; set; }

    public int? ExtraFee { get; set; }

    public int? Weight { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
