using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Route")]
public partial class Route
{
    [Key]
    [Column("RouteID")]
    public int RouteId { get; set; }

    [Column("DepartureAirportID")]
    public int DepartureAirportId { get; set; }

    [Column("ArrivalAirportID")]
    public int ArrivalAirportId { get; set; }

    public TimeOnly Duration { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal BasePrice { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; } = "Active";

    [ForeignKey("ArrivalAirportId")]
    [InverseProperty("RouteArrivalAirports")]
    public virtual Airport ArrivalAirport { get; set; } = null!;

    [ForeignKey("DepartureAirportId")]
    [InverseProperty("RouteDepartureAirports")]
    public virtual Airport DepartureAirport { get; set; } = null!;

    [InverseProperty("Route")]
    public virtual ICollection<FlightSchedule> FlightSchedules { get; set; } = new List<FlightSchedule>();
}
