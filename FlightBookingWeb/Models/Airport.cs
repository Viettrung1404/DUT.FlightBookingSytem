using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Airport
{
    public int AirportId { get; set; }

    public string AirportCode { get; set; } = null!;

    public string AirportName { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<Route> RouteArrivalAirports { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteDepartureAirports { get; set; } = new List<Route>();
}
