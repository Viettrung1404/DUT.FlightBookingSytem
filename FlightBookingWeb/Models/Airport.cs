using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Airport
{
    public int AirportId { get; set; }

    public string? AirportName { get; set; }

    public string? City { get; set; }

    public virtual ICollection<FlightRoute> FlightRouteArrivalAirports { get; set; } = new List<FlightRoute>();

    public virtual ICollection<FlightRoute> FlightRouteDepartureAirports { get; set; } = new List<FlightRoute>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
