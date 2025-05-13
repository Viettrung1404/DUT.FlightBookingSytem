using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Ticket")]
public partial class Ticket
{
    [Key]
    [Column("TicketID")]
    public int TicketId { get; set; }

    [Column("FlightID")]
    public int FlightId { get; set; }

    [Column("AccountID")]
    public int AccountId { get; set; }

    [Column("SeatID")]
    public int? SeatId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BookingDate { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Tickets")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Ticket")]
    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();

    [ForeignKey("FlightId")]
    [InverseProperty("Tickets")]
    public virtual Flight Flight { get; set; } = null!;

    [InverseProperty("Ticket")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("SeatId")]
    [InverseProperty("Tickets")]
    public virtual Seat? Seat { get; set; }
}
