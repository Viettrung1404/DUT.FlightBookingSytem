using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;
public partial class Ticket
{
    public int TicketId { get; set; }
    public int FlightId { get; set; }
    public int AccountId { get; set; }
    public int? SeatId { get; set; }
    public decimal Price { get; set; }
    public DateTime? BookingDate { get; set; }
    public string? Status { get; set; }
    public string TicketType { get; set; } = null!;
    public int? RelatedTicketId { get; set; }
    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();
    public virtual Flight Flight { get; set; } = null!;
    public virtual ICollection<Ticket> InverseRelatedTicket { get; set; } = new List<Ticket>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual Ticket? RelatedTicket { get; set; }
    public virtual Seat? Seat { get; set; }
}
