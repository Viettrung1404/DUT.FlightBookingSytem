using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Flight
{
    public int FlightId { get; set; }

    public int? RouteId { get; set; }

    public int? AirplaneId { get; set; }

    public string? Status { get; set; }

    public TimeOnly? DepartureTime { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public virtual Airplane? Airplane { get; set; }

    public virtual FlightRoute? Route { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
