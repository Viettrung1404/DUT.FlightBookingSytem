using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Baggage
{
    public int BaggageId { get; set; }

    public int? CustomerId { get; set; }

    public int? TicketId { get; set; }

    public int? ExtraFee { get; set; }

    public int? Weight { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
