using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class FlightRoute
{
    public string RouteId { get; set; } = null!;

    public string? DepartureAirportId { get; set; }

    public string? ArrivalAirportId { get; set; }

    public virtual Airport? ArrivalAirport { get; set; }

    public virtual Airport? DepartureAirport { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
