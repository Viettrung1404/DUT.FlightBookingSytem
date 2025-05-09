using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Flight
{
    public int FlightId { get; set; }

    public int ScheduleId { get; set; }

    public int AirplaneId { get; set; }

    public DateTime DepartureDateTime { get; set; }

    public DateTime ArrivalDateTime { get; set; }

    public string? Status { get; set; }

    public virtual Airplane Airplane { get; set; } = null!;

    public virtual FlightSchedule Schedule { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
