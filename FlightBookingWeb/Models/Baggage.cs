using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Baggage")]
public partial class Baggage
{
    [Key]
    [Column("BaggageID")]
    public int BaggageId { get; set; }

    [Column("TicketID")]
    public int TicketId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Weight { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Fee { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("Baggages")]
    public virtual Ticket Ticket { get; set; } = null!;
}
