using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Flight")]
public partial class Flight
{
    [Key]
    [Column("FlightID")]
    public int FlightId { get; set; }

    [Column("ScheduleID")]
    public int ScheduleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DepartureDateTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ArrivalDateTime { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("ScheduleId")]
    [InverseProperty("Flights")]
    public virtual FlightSchedule Schedule { get; set; } = null!;

    [InverseProperty("Flight")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
