using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Airplane
{
    public string AirplaneId { get; set; } = null!;

    public string? AirplaneName { get; set; }

    public string? AirplaneType { get; set; }

    public int? EconomySeats { get; set; }

    public int? Vipseats { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
