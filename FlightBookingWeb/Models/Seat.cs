using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Seat")]
[Index("AirplaneId", "SeatNumber", Name = "UQ_Seat_Airplane", IsUnique = true)]
public partial class Seat
{
    [Key]
    [Column("SeatID")]
    public int SeatId { get; set; }

    [Column("AirplaneID")]
    public int AirplaneId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string SeatNumber { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string SeatClass { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string SeatType { get; set; } = null!;

    public bool? IsEmergencyExit { get; set; }

    [ForeignKey("AirplaneId")]
    [InverseProperty("Seats")]
    public virtual Airplane Airplane { get; set; } = null!;

    [InverseProperty("Seat")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
