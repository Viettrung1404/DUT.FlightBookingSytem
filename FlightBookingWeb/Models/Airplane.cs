using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Airplane
{
    public int AirplaneId { get; set; }

    public string AirplaneName { get; set; } = null!;

    public string AirplaneType { get; set; } = null!;

    public int TotalSeats { get; set; }

    public int EconomySeats { get; set; }

    public int BusinessSeats { get; set; }

    public int? ManufactureYear { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<FlightSchedule> FlightSchedules { get; set; } = new List<FlightSchedule>();

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
