using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int TicketId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string? Status { get; set; }

    public string? TransactionId { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Ticket Ticket { get; set; } = null!;
}
