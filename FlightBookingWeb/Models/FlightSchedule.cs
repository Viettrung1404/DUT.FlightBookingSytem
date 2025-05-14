using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("FlightSchedule")]
public partial class FlightSchedule
{
    [Key]
    [Column("ScheduleID")]
    public int ScheduleId { get; set; }

    [Column("AirplaneID")]
    public int? AirplaneId { get; set; }

    [Column("RouteID")]
    public int RouteId { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Frequency { get; set; } = null!;

    public bool? Active { get; set; }

    [ForeignKey("AirplaneId")]
    [InverseProperty("FlightSchedules")]
    public virtual Airplane? Airplane { get; set; }

    [InverseProperty("Schedule")]
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    [ForeignKey("RouteId")]
    [InverseProperty("FlightSchedules")]
    public virtual Route Route { get; set; } = null!;
}
