using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int? CustomerId { get; set; }

    public string? IssueDate { get; set; }

    public int? TicketId { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
