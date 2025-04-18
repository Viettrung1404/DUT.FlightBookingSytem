using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Flight
{
    public string FlightId { get; set; } = null!;

    public string? RouteId { get; set; }

    public string? AirplaneId { get; set; }

    public string? Status { get; set; }

    public TimeOnly? DepartureTime { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public virtual Airplane? Airplane { get; set; }

    public virtual FlightRoute? Route { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
