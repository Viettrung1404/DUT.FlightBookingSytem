using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Airport")]
[Index("AirportCode", Name = "UQ__Airport__4B677353626E9276", IsUnique = true)]
public partial class Airport
{
    [Key]
    [Column("AirportID")]
    public int AirportId { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string AirportCode { get; set; } = null!;

    [StringLength(100)]
    public string AirportName { get; set; } = null!;

    [StringLength(50)]
    public string City { get; set; } = null!;

    [StringLength(50)]
    public string Country { get; set; } = null!;

    [InverseProperty("ArrivalAirport")]
    public virtual ICollection<Route> RouteArrivalAirports { get; set; } = new List<Route>();

    [InverseProperty("DepartureAirport")]
    public virtual ICollection<Route> RouteDepartureAirports { get; set; } = new List<Route>();
}
