using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class FlightRoute
{
    public int RouteId { get; set; }

    public int? DepartureAirportId { get; set; }

    public int? ArrivalAirportId { get; set; }

    public virtual Airport? ArrivalAirport { get; set; }

    public virtual Airport? DepartureAirport { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
