using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Airplane")]
public partial class Airplane
{
    [Key]
    [Column("AirplaneID")]
    public int AirplaneId { get; set; }

    [StringLength(50)]
    public string AirplaneName { get; set; } = null!;

    [StringLength(50)]
    public string AirplaneType { get; set; } = null!;

    public int TotalSeats { get; set; }

    public int EconomySeats { get; set; }

    public int BusinessSeats { get; set; }

    public int? ManufactureYear { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [InverseProperty("Airplane")]
    public virtual ICollection<FlightSchedule> FlightSchedules { get; set; } = new List<FlightSchedule>();

    [InverseProperty("Airplane")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
