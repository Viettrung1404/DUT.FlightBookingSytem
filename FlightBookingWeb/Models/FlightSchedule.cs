using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class FlightSchedule
{
    public int ScheduleId { get; set; }

    public int? AirplaneId { get; set; }

    public int RouteId { get; set; }

    public DateTime DepartureTime { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    public int Frequency { get; set; }

    public bool? Active { get; set; }

    public bool? Status { get; set; }

    public virtual Airplane? Airplane { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual Route Route { get; set; } = null!;
}
