using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Invoice")]
[Index("InvoiceNumber", Name = "UQ__Invoice__D776E9814C31ADD4", IsUnique = true)]
public partial class Invoice
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string InvoiceNumber { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? IssueDate { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalAmount { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("Invoices")]
    public virtual Payment Payment { get; set; } = null!;
}
