using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class TicketClass
{
    public int ClassId { get; set; }

    public string? ClassName { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
