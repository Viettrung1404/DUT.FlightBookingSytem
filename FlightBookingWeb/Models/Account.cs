using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Models;

[Table("Account")]
[Index("Username", Name = "UQ__Account__536C85E470B2C223", IsUnique = true)]
[Index("Email", Name = "UQ__Account__A9D105341B7701DA", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("AccountID")]
    public int AccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    public bool? Gender { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [InverseProperty("Account")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
