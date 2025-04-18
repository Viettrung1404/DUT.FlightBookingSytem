using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class TicketClass
{
    public string ClassId { get; set; } = null!;

    public string? ClassName { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
