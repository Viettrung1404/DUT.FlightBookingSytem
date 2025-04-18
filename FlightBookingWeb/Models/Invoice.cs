using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Invoice
{
    public string InvoiceId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? IssueDate { get; set; }

    public string? TicketId { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
