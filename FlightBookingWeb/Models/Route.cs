using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Route
{
    public int RouteId { get; set; }

    public int DepartureAirportId { get; set; }

    public int ArrivalAirportId { get; set; }

    public TimeOnly Duration { get; set; }

    public decimal BasePrice { get; set; }

    public virtual Airport ArrivalAirport { get; set; } = null!;

    public virtual Airport DepartureAirport { get; set; } = null!;

    public virtual ICollection<FlightSchedule> FlightSchedules { get; set; } = new List<FlightSchedule>();
}
