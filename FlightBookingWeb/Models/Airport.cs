using System;
using System.Collections.Generic;

namespace FlightBookingWeb.Models;

public partial class Airport
{
    public int AirportId { get; set; }

    public string AirportCode { get; set; } = null!;

    public string AirportName { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;
}
