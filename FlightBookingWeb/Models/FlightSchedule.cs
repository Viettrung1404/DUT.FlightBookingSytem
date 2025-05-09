using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class FlightSchedule
{
    public int ScheduleId { get; set; }

    public int RouteId { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    public string Frequency { get; set; } = null!;

    public bool? Active { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual Route Route { get; set; } = null!;
}
