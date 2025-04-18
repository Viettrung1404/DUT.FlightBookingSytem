using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class MonthlyRevenue
{
    public string RevenueId { get; set; } = null!;

    public int? Revenue { get; set; }

    public int? TicketsSold { get; set; }
}
