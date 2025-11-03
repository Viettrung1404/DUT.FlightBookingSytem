using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class VwFlightDetail
{
    public int FlightId { get; set; }

    public int RouteId { get; set; }

    public string DepartureAirport { get; set; } = null!;

    public string DepartureAirportName { get; set; } = null!;

    public string DepartureCity { get; set; } = null!;

    public string ArrivalAirport { get; set; } = null!;

    public string ArrivalAirportName { get; set; } = null!;

    public string ArrivalCity { get; set; } = null!;

    public DateTime DepartureDateTime { get; set; }

    public DateTime ArrivalDateTime { get; set; }

    public int? DurationMinutes { get; set; }

    public string AirplaneName { get; set; } = null!;

    public string AirplaneType { get; set; } = null!;

    public string? Status { get; set; }
}
