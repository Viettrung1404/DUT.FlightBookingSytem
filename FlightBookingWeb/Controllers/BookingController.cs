using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.Service;
using FlightBookingWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FlightBookingWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly IPayPalService _payPalService;
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context, IPayPalService payPalService)
        {
            _context = context;
            _payPalService = payPalService;
        }
        public IActionResult Search()
        {

            var cities = _context.Airports
                .Select(a => a.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            var viewModel = new FlightViewModel
            {
                DepartureOutBoardDate = DateTime.Now,
                Cities = cities
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SearchResults(FlightViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Cities = _context.Airports
                        .Select(a => a.City)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                    return View("Search", model);
                }
                var outboundFlights = _context.Flights
                    .Include(f => f.Schedule)
                    .ThenInclude(s => s.Route)
                    .Where(f => f.Schedule.Route.DepartureAirport.City.Contains(model.DepartureAirport)
                             && f.Schedule.Route.ArrivalAirport.City.Contains(model.ArrivalAirport)
                             && f.DepartureDateTime.Date == model.DepartureOutBoardDate.Date)
                    .Select(f => new FlightViewModel
                    {
                        FlightId = f.FlightId,
                        DepartureAirport = f.Schedule.Route.DepartureAirport.AirportName,
                        ArrivalAirport = f.Schedule.Route.ArrivalAirport.AirportName,
                        DepartureOutBoardDate = f.DepartureDateTime,
                        Price = (decimal?)_context.Routes
                            .Where(r => r.RouteId == f.Schedule.RouteId)
                            .Select(r => r.BasePrice)
                            .FirstOrDefault()
                    })
                    .ToList();

                List<FlightViewModel> returnFlights = null;
                if (model.IsRoundTrip && model.DepartureReturnDate.HasValue)
                {
                    var returnDate = model.DepartureReturnDate.Value.Date;
                    returnFlights = _context.Flights
                        .Include(f => f.Schedule)
                            .ThenInclude(s => s.Route)
                        .Where(f => f.DepartureDateTime.Date == returnDate && f.Schedule.Route.DepartureAirport.City == model.DepartureAirport && f.Schedule.Route.ArrivalAirport.City == model.ArrivalAirport)
                        .Select(f => new FlightViewModel
                        {
                            FlightId = f.FlightId,
                            DepartureAirport = f.Schedule.Route.DepartureAirport.AirportName,
                            ArrivalAirport = f.Schedule.Route.ArrivalAirport.AirportName,
                            DepartureOutBoardDate = f.DepartureDateTime,
                            Price = (decimal?)_context.Routes
                                .Where(r => r.RouteId == f.Schedule.RouteId)
                                .Select(r => r.BasePrice)
                                .FirstOrDefault()
                        })
                        .ToList();
                }

                var viewModel = new FlightSearchResultsViewModel
                {
                    OutboundFlights = outboundFlights,
                    ReturnFlights = returnFlights,
                    IsRoundTrip = model.IsRoundTrip,
                    PassengerCount = model.PassengerCount
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public IActionResult SelectSeats(int outboundFlightId, int? returnFlightId, int passengerCount)
        {
            // Lấy thông tin chuyến bay đi
            var outboundFlight = _context.Flights
                .Include(f => f.Schedule)
                .ThenInclude(s => s.Airplane)
                .FirstOrDefault(f => f.FlightId == outboundFlightId);

            if (outboundFlight == null)
            {
                return NotFound();
            }

            // Lấy danh sách ghế đã đặt của chuyến bay đi
            var bookedOutboundSeatIds = _context.SeatBookings
                .Where(sb => sb.FlightId == outboundFlightId && sb.IsBooked)
                .Select(sb => sb.SeatId)
                .ToList();

            // Lấy danh sách ghế máy bay cho chuyến bay đi
            var availableSeats = _context.Seats
                .Where(seat => seat.AirplaneId == outboundFlight.Schedule.AirplaneId)
                .Select(seat => new SeatViewModel
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    SeatClass = seat.SeatClass,
                    SeatType = seat.SeatType,
                    IsBooked = bookedOutboundSeatIds.Contains(seat.SeatId)
                })
                .ToList();

            // Nếu có chuyến về, xử lý tương tự
            List<SeatViewModel> returnSeats = null;
            if (returnFlightId.HasValue)
            {
                var bookedReturnSeatIds = _context.SeatBookings
                    .Where(sb => sb.FlightId == returnFlightId && sb.IsBooked)
                    .Select(sb => sb.SeatId)
                    .ToList();

                returnSeats = _context.Seats
                    .Where(seat => seat.AirplaneId == outboundFlight.Schedule.AirplaneId)
                    .Select(seat => new SeatViewModel
                    {
                        SeatId = seat.SeatId,
                        SeatNumber = seat.SeatNumber,
                        SeatClass = seat.SeatClass,
                        SeatType = seat.SeatType,
                        IsBooked = bookedReturnSeatIds.Contains(seat.SeatId)
                    }).ToList();
            }

            var viewModel = new SeatSelectionViewModel
            {
                OutboundFlightId = outboundFlightId,
                ReturnFlightId = returnFlightId,
                PassengerCount = passengerCount,
                IsRoundTrip = returnFlightId.HasValue,
                AvailableOutboundSeats = availableSeats,
                AvailableReturnSeats = returnSeats
            };
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmSeat(SeatSelectionViewModel model)
        {
            if ((model.SelectedSeatsOutBoard == null || model.SelectedSeatsOutBoard.Count != model.PassengerCount) ||
                (model.IsRoundTrip && (model.SelectedSeatsReturnBoard == null || model.SelectedSeatsReturnBoard.Count != model.PassengerCount)))
            {
                TempData["Error"] = "Vui lòng chọn đúng số lượng ghế cho mỗi chiều.";
                return RedirectToAction("SelectSeats", new
                {
                    outboundFlightId = model.OutboundFlightId,
                    returnFlightId = model.ReturnFlightId,
                    passengerCount = model.PassengerCount
                });
            }

            // Lấy AirplaneId của chuyến bay đi
            var outboundFlight = _context.Flights
                .Include(f => f.Schedule)
                .FirstOrDefault(f => f.FlightId == model.OutboundFlightId);
            int? outboundAirplaneId = outboundFlight?.Schedule.AirplaneId;

            // Lấy AirplaneId của chuyến bay về (nếu có)
            int? returnAirplaneId = null;
            if (model.IsRoundTrip && model.ReturnFlightId.HasValue)
            {
                var returnFlight = _context.Flights
                    .Include(f => f.Schedule)
                    .FirstOrDefault(f => f.FlightId == model.ReturnFlightId.Value);
                returnAirplaneId = returnFlight?.Schedule.AirplaneId;
            }

            // Truy vấn chi tiết các ghế chiều đi
            var outboundSeats = _context.Seats
                .Where(s => s.AirplaneId == outboundAirplaneId && model.SelectedSeatsOutBoard.Contains(s.SeatNumber))
                .Select(s => new SeatViewModel
                {
                    SeatId = s.SeatId,
                    SeatNumber = s.SeatNumber,
                    SeatClass = s.SeatClass,
                    SeatType = s.SeatType,
                    IsBooked = true
                }).ToList();

            // Truy vấn chi tiết các ghế chiều về (nếu có)
            List<SeatViewModel> returnSeats = new();
            if (model.IsRoundTrip && model.SelectedSeatsReturnBoard != null && returnAirplaneId.HasValue)
            {
                returnSeats = _context.Seats
                    .Where(s => s.AirplaneId == returnAirplaneId && model.SelectedSeatsReturnBoard.Contains(s.SeatNumber))
                    .Select(s => new SeatViewModel
                    {
                        SeatId = s.SeatId,
                        SeatNumber = s.SeatNumber,
                        SeatClass = s.SeatClass,
                        SeatType = s.SeatType,
                        IsBooked = true
                    }).ToList();
            }

            var viewModel = new SeatConfirmViewModel
            {
                OutboundFlightId = model.OutboundFlightId,
                ReturnFlightId = model.ReturnFlightId,
                PassengerCount = model.PassengerCount,
                IsRoundTrip = model.IsRoundTrip,
                SelectedOutboundSeats = outboundSeats,
                SelectedReturnSeats = returnSeats
            };

            return View("ConfirmSeat", viewModel);
        }

        [HttpGet]
        public IActionResult Checkout(int outboundFlightId, int? returnFlightId, int passengerCount, List<string> selectedSeatsOutBoard, List<string>? selectedSeatsReturnBoard)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    // Prepare data to store in cookie
                    var checkoutData = new
                    {
                        outboundFlightId,
                        returnFlightId,
                        passengerCount,
                        selectedSeatsOutBoard,
                        selectedSeatsReturnBoard
                    };

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                        IsEssential = true,
                        HttpOnly = true,
                        Secure = true // Set to true in production
                    };

                    Response.Cookies.Append(
                        "PreLoginCheckoutData",
                        JsonConvert.SerializeObject(checkoutData),
                        cookieOptions
                    );
                    var Urlreturn = Url.Action("Checkout", "Booking");
                    return RedirectToAction("Login", "Account", Urlreturn);
                }

                // Lấy thông tin chuyến bay đi
                var outboundFlight = _context.Flights
                    .Include(f => f.Schedule)
                        .ThenInclude(s => s.Route)
                    .FirstOrDefault(f => f.FlightId == outboundFlightId);

                if (outboundFlight == null)
                {
                    TempData["Error"] = "Không tìm thấy chuyến bay đi.";
                    return RedirectToAction("Search");
                }

                // Lấy thông tin sân bay đi và đến
                var departureAirport = _context.Airports.FirstOrDefault(a => a.AirportId == outboundFlight.Schedule.Route.DepartureAirportId);
                var arrivalAirport = _context.Airports.FirstOrDefault(a => a.AirportId == outboundFlight.Schedule.Route.ArrivalAirportId);

                // Tính giá từng ghế chiều đi
                var outboundBasePrice = _context.Routes
                    .Where(r => r.RouteId == outboundFlight.Schedule.RouteId)
                    .Select(r => r.BasePrice)
                    .FirstOrDefault();

                decimal totalAmount = 0;

                if (selectedSeatsOutBoard != null)
                {
                    foreach (var seatNumber in selectedSeatsOutBoard)
                    {
                        var seat = _context.Seats.FirstOrDefault(s => s.SeatNumber == seatNumber && s.AirplaneId == outboundFlight.Schedule.AirplaneId);
                        if (seat != null)
                        {
                            totalAmount += SeatPricingHelper.CalculateSeatPrice(outboundBasePrice, seat.SeatClass, seat.SeatType);
                        }
                    }
                }

                // Xử lý chiều về (nếu có)
                Flight? returnFlight = null;
                string? returnDepartureCity = null;
                string? returnArrivalCity = null;

                if (returnFlightId.HasValue)
                {
                    returnFlight = _context.Flights
                        .Include(f => f.Schedule)
                            .ThenInclude(s => s.Route)
                        .FirstOrDefault(f => f.FlightId == returnFlightId);

                    if (returnFlight != null && selectedSeatsReturnBoard != null)
                    {
                        var returnBasePrice = _context.Routes
                            .Where(r => r.RouteId == returnFlight.Schedule.RouteId)
                            .Select(r => r.BasePrice)
                            .FirstOrDefault();

                        foreach (var seatNumber in selectedSeatsReturnBoard)
                        {
                            var seat = _context.Seats.FirstOrDefault(s => s.SeatNumber == seatNumber && s.AirplaneId == returnFlight.Schedule.AirplaneId);
                            if (seat != null)
                            {
                                totalAmount += SeatPricingHelper.CalculateSeatPrice(returnBasePrice, seat.SeatClass, seat.SeatType);
                            }
                        }

                        // Lấy thông tin sân bay cho chuyến về
                        var returnDepAirport = _context.Airports.FirstOrDefault(a => a.AirportId == returnFlight.Schedule.Route.DepartureAirportId);
                        var returnArrAirport = _context.Airports.FirstOrDefault(a => a.AirportId == returnFlight.Schedule.Route.ArrivalAirportId);
                        returnDepartureCity = returnDepAirport?.City;
                        returnArrivalCity = returnArrAirport?.City;
                    }
                }

                // Tạo view model để hiển thị thông tin xác nhận
                var viewModel = new CheckoutViewModel
                {
                    OutboundFlight = outboundFlight,
                    ReturnFlight = returnFlight,
                    SelectedOutboundSeats = selectedSeatsOutBoard,
                    SelectedReturnSeats = selectedSeatsReturnBoard,
                    TotalAmount = totalAmount,
                    PassengerCount = passengerCount,
                    DepartureCity = departureAirport?.City,
                    ArrivalCity = arrivalAirport?.City,
                    ReturnDepartureCity = returnDepartureCity,
                    ReturnArrivalCity = returnArrivalCity
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Đã xảy ra lỗi khi tải trang xác nhận.");
            }
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (model.Passengers == null || model.SelectedOutboundSeats == null)
            {
                ViewBag.ShowPayPalButton = false;
                return View(model);
            }

            // Tính tổng tiền vé
            decimal totalAmount = model.TotalAmount;

            // Tính tiền hành lý
            decimal baggageTotal = 0;
            foreach (var passenger in model.Passengers)
            {
                if (passenger.ExtraBaggageKg == 5)
                    baggageTotal += 20; // Giá 5kg
                else if (passenger.ExtraBaggageKg == 10)
                    baggageTotal += 35; // Giá 10kg
            }

            totalAmount += baggageTotal;
            model.TotalAmount = totalAmount;

            // Lưu vào session
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            string jsonData = JsonConvert.SerializeObject(model, settings);
            HttpContext.Session.SetString("CheckoutData", jsonData);

            ViewBag.ShowPayPalButton = true;
            return View(model);
        }


        #region PayPal payment

        [HttpPost("booking/create-paypal-order")]
        public async Task<IActionResult> CreatePayPalOrder()
        {
            try
            {
                var sessionData = HttpContext.Session.GetString("CheckoutData");
                if (string.IsNullOrEmpty(sessionData))
                    return BadRequest("No checkout data found in session.");

                var checkoutData = JsonConvert.DeserializeObject<CheckoutViewModel>(sessionData);
                if (checkoutData == null)
                    return BadRequest("Invalid checkout data.");

                var amount = checkoutData.TotalAmount;
                var currency = "USD";
                var description = $"Flight booking for {checkoutData.PassengerCount} passenger(s)";

                // Gọi dịch vụ tạo đơn hàng PayPal, trả về orderId
                var orderId = await _payPalService.CreateOrderAsync(amount, currency, description);

                // Trả về orderId cho JS
                return Ok(new { id = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PayPal Order Error: {ex.Message}");
                return BadRequest("Could not create PayPal order.");
            }
        }


        [HttpPost("booking/capture-paypal-order")]
        public async Task<IActionResult> CapturePayPalOrder([FromQuery] string orderId)
        {
            try
            {
                // Capture PayPal payment
                var result = await _payPalService.CapturePaymentAsync(orderId);
                if (!result)
                    return BadRequest("Failed to capture PayPal order.");

                // Get checkout data from session
                var sessionData = HttpContext.Session.GetString("CheckoutData");
                if (string.IsNullOrEmpty(sessionData))
                    return BadRequest("No checkout data found in session.");

                var checkoutData = JsonConvert.DeserializeObject<CheckoutViewModel>(sessionData);
                if (checkoutData == null)
                    return BadRequest("Invalid checkout data.");

                var accountId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                // Prepare ticket lists for outbound and return
                var outboundTickets = new List<Ticket>();
                var returnTickets = new List<Ticket>();
                var allTickets = new List<Ticket>();

                // Outbound airplane
                var airplaneOutboardId = _context.Flights
                    .Include(f => f.Schedule)
                    .Where(f => f.FlightId == checkoutData.OutboundFlight.FlightId)
                    .Select(f => f.Schedule.AirplaneId)
                    .FirstOrDefault();
                if (airplaneOutboardId == null)
                    return BadRequest("Outbound airplane not found.");

                // Create outbound tickets and seat bookings
                for (int i = 0; i < checkoutData.SelectedOutboundSeats.Count; i++)
                {
                    var seatCode = checkoutData.SelectedOutboundSeats[i];
                    var seat = _context.Seats.FirstOrDefault(
                        s => s.SeatNumber == seatCode && s.AirplaneId == airplaneOutboardId);
                    if (seat == null) continue;

                    var passenger = (checkoutData.Passengers != null && i < checkoutData.Passengers.Count)
                        ? checkoutData.Passengers[i]
                        : null;

                    var ticket = new Ticket
                    {
                        FlightId = checkoutData.OutboundFlight.FlightId,
                        AccountId = accountId,
                        SeatId = seat.SeatId,
                        Price = checkoutData.TotalAmount / checkoutData.PassengerCount,
                        BookingDate = DateTime.UtcNow,
                        Status = "Confirmed",
                        TicketType = "Outbound"
                    };
                    outboundTickets.Add(ticket);
                    allTickets.Add(ticket);

                    var seatBooking = new SeatBooking
                    {
                        FlightId = ticket.FlightId,
                        SeatId = seat.SeatId,
                        AccountId = accountId,
                        IsBooked = true,
                        BookingDate = DateTime.UtcNow,
                        FullName = passenger?.FullName,
                        Email = passenger?.Email,
                        PhoneNumber = passenger?.PhoneNumber,
                        CountryCode = passenger?.CountryCode,
                        PassportNumber = passenger?.PassportNumber,
                        Cccd = passenger?.CCCD,
                        DateOfBirth = passenger?.DateOfBirth != null
                            ? DateOnly.FromDateTime(passenger.DateOfBirth)
                            : (DateOnly?)null
                    };
                    _context.SeatBookings.Add(seatBooking);
                }

                // Create return tickets and seat bookings (if any)
                if (checkoutData.ReturnFlight != null && checkoutData.SelectedReturnSeats != null)
                {
                    var returnAirplaneId = _context.Flights
                        .Include(f => f.Schedule)
                        .Where(f => f.FlightId == checkoutData.ReturnFlight.FlightId)
                        .Select(f => f.Schedule.AirplaneId)
                        .FirstOrDefault();
                    if (returnAirplaneId == null)
                        return BadRequest("Return airplane not found.");

                    for (int i = 0; i < checkoutData.SelectedReturnSeats.Count; i++)
                    {
                        var seatCode = checkoutData.SelectedReturnSeats[i];
                        var seat = _context.Seats.FirstOrDefault(
                            s => s.SeatNumber == seatCode && s.AirplaneId == returnAirplaneId);
                        if (seat == null) continue;

                        var passenger = (checkoutData.Passengers != null && i < checkoutData.Passengers.Count)
                            ? checkoutData.Passengers[i]
                            : null;

                        var ticket = new Ticket
                        {
                            FlightId = checkoutData.ReturnFlight.FlightId,
                            AccountId = accountId,
                            SeatId = seat.SeatId,
                            Price = checkoutData.TotalAmount / checkoutData.PassengerCount,
                            BookingDate = DateTime.UtcNow,
                            Status = "Confirmed",
                            TicketType = "Return"
                        };
                        returnTickets.Add(ticket);
                        allTickets.Add(ticket);

                        var seatBooking = new SeatBooking
                        {
                            FlightId = ticket.FlightId,
                            SeatId = seat.SeatId,
                            AccountId = accountId,
                            IsBooked = true,
                            BookingDate = DateTime.UtcNow,
                            FullName = passenger?.FullName,
                            Email = passenger?.Email,
                            PhoneNumber = passenger?.PhoneNumber,
                            CountryCode = passenger?.CountryCode,
                            PassportNumber = passenger?.PassportNumber,
                            Cccd = passenger?.CCCD,
                            DateOfBirth = passenger?.DateOfBirth != null
                                ? DateOnly.FromDateTime(passenger.DateOfBirth)
                                : (DateOnly?)null
                        };
                        _context.SeatBookings.Add(seatBooking);
                    }
                }

                // Save all tickets to DB to get TicketId
                _context.Tickets.AddRange(allTickets);
                await _context.SaveChangesAsync();

                // Link outbound and return tickets by RelatedTicketId
                if (outboundTickets.Count == returnTickets.Count && outboundTickets.Count > 0)
                {
                    for (int i = 0; i < outboundTickets.Count; i++)
                    {
                        outboundTickets[i].RelatedTicketId = returnTickets[i].TicketId;
                        returnTickets[i].RelatedTicketId = outboundTickets[i].TicketId;
                    }
                    _context.Tickets.UpdateRange(outboundTickets);
                    _context.Tickets.UpdateRange(returnTickets);
                    await _context.SaveChangesAsync();
                }

                // Save payment and invoice for each ticket
                foreach (var ticket in allTickets)
                {
                    var payment = new Payment
                    {
                        TicketId = ticket.TicketId,
                        Amount = ticket.Price,
                        PaymentMethod = "Paypal Payment",
                        PaymentDate = DateTime.UtcNow,
                        Status = "Completed",
                        TransactionId = orderId
                    };

                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync(); // Save to get PaymentId

                    var invoice = new Invoice
                    {
                        PaymentId = payment.PaymentId,
                        InvoiceNumber = "INV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        IssueDate = DateTime.UtcNow,
                        TaxAmount = Math.Round(payment.Amount * 0.1m, 2),
                        TotalAmount = payment.Amount
                    };

                    _context.Invoices.Add(invoice);
                }

                await _context.SaveChangesAsync();

                // Assign baggage for each ticket if any
                if (checkoutData.Passengers != null)
                {
                    for (int i = 0; i < allTickets.Count && i < checkoutData.Passengers.Count; i++)
                    {
                        var ticket = allTickets[i];
                        var passenger = checkoutData.Passengers[i];
                        if (passenger != null && passenger.ExtraBaggageKg > 0)
                        {
                            var baggage = new Baggage
                            {
                                TicketId = ticket.TicketId,
                                Weight = passenger.ExtraBaggageKg,
                                Status = "Booked",
                            };
                            _context.Baggages.Add(baggage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Clear session
                HttpContext.Session.Remove("CheckoutData");

                return Ok(new { message = "Payment captured and booking completed successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                // Log full details for debugging
                Console.WriteLine("DbUpdateException: " + dbEx.ToString());
                if (dbEx.InnerException != null)
                    Console.WriteLine("InnerException: " + dbEx.InnerException.ToString());
                return StatusCode(500, "Database error when saving tickets. Please check required fields and constraints.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
                return StatusCode(500, "Unknown error when saving tickets.");
            }
        }




        #endregion


    }


}
