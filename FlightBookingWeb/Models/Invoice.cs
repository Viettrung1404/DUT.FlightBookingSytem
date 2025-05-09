using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int PaymentId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime? IssueDate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
