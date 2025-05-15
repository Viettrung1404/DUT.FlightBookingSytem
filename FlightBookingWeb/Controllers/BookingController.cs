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
                var outboundFlights = _context.Flights
                    .Include(f => f.Schedule)
                    .ThenInclude(s => s.Route)
                    .Where(f => f.Schedule.Route.DepartureAirport.City.Contains(model.DepartureAirport)
                             && f.Schedule.Route.ArrivalAirport.City.Contains(model.ArrivalAirport)
                             && f.DepartureDateTime.Date == model.DepartureOutBoardDate)
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
                    returnFlights = _context.Flights
                        .Include(f => f.Schedule)
                        .ThenInclude(s => s.Route)
                        .Where(f => f.Schedule.Route.DepartureAirport.City.Contains(model.ArrivalAirport)
                                 && f.Schedule.Route.ArrivalAirport.City.Contains(model.DepartureAirport)
                                 && f.DepartureDateTime.Date == model.DepartureReturnDate.Value.Date)
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
                // Log the exception (use a logging framework like Serilog, NLog, etc.)
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

            // Truy vấn chi tiết các ghế
            var outboundSeats = _context.Seats
                .Where(s => model.SelectedSeatsOutBoard.Contains(s.SeatNumber))
                .Select(s => new SeatViewModel
                {
                    SeatId = s.SeatId,
                    SeatNumber = s.SeatNumber,
                    SeatClass = s.SeatClass,
                    SeatType = s.SeatType,
                    IsBooked = true
                }).ToList();

            List<SeatViewModel> returnSeats = new();
            if (model.IsRoundTrip)
            {
                returnSeats = _context.Seats
                    .Where(s => model.SelectedSeatsReturnBoard.Contains(s.SeatNumber))
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
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30), // Set expiration as needed
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

                // Tính giá chuyến bay đi
                var outboundPrice = _context.Routes
                    .Where(r => r.RouteId == outboundFlight.Schedule.RouteId)
                    .Select(r => r.BasePrice)
                    .FirstOrDefault();

                decimal totalAmount = outboundPrice * (selectedSeatsOutBoard?.Count ?? 0);
                decimal returnPrice = 0;
                Flight? returnFlight = null;
                string? returnDepartureCity = null;
                string? returnArrivalCity = null;

                // Nếu có chuyến bay về, xử lý tương tự
                if (returnFlightId.HasValue)
                {
                    returnFlight = _context.Flights
                        .Include(f => f.Schedule)
                            .ThenInclude(s => s.Route)
                        .FirstOrDefault(f => f.FlightId == returnFlightId);

                    if (returnFlight != null)
                    {
                        returnPrice = _context.Routes
                            .Where(r => r.RouteId == returnFlight.Schedule.RouteId)
                            .Select(r => r.BasePrice)
                            .FirstOrDefault();

                        totalAmount += returnPrice * (selectedSeatsReturnBoard?.Count ?? 0);

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

            // Lưu vào session
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // hoặc .Serialize
                PreserveReferencesHandling = PreserveReferencesHandling.Objects // nếu muốn giữ reference
            };
            string jsonData = JsonConvert.SerializeObject(model, settings);
            HttpContext.Session.SetString("CheckoutData", jsonData);

            // Hiển thị lại view với nút PayPal 
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
                // Gọi service để capture (hoàn tất thanh toán)
                var result = await _payPalService.CapturePaymentAsync(orderId);
                if (!result)
                {
                    return BadRequest("Failed to capture PayPal order.");
                }

                // Lấy dữ liệu từ session
                var sessionData = HttpContext.Session.GetString("CheckoutData");
                if (string.IsNullOrEmpty(sessionData))
                {
                    return BadRequest("No checkout data found in session.");
                }

                var checkoutData = JsonConvert.DeserializeObject<CheckoutViewModel>(sessionData);
                if (checkoutData == null)
                {
                    return BadRequest("Invalid checkout data.");
                }

                var accountId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var tickets = new List<Ticket>();

                // Xử lý vé chiều đi
                var airplaneOutboardId = _context.Flights
                    .Include(f => f.Schedule)
                    .Where(f => f.FlightId == checkoutData.OutboundFlight.FlightId)
                    .Select(f => f.Schedule.AirplaneId)
                    .FirstOrDefault();
                if (airplaneOutboardId == null)
                    return BadRequest("Outbound airplane not found.");

                foreach (var seatCode in checkoutData.SelectedOutboundSeats)
                {
                    var seat = _context.Seats.FirstOrDefault(
                        s => s.SeatNumber == seatCode && s.AirplaneId == airplaneOutboardId);
                    if (seat == null) continue;

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
                    ticket.Flight = _context.Flights.Where(f => f.FlightId == ticket.FlightId).FirstOrDefault();
                    ticket.Account = _context.Accounts.Where(f => f.AccountId == ticket.AccountId).FirstOrDefault();
                    tickets.Add(ticket);

                    var seatBooking = new SeatBooking
                    {
                        FlightId = ticket.FlightId,
                        SeatId = seat.SeatId,
                        AccountId = accountId,
                        IsBooked = true,
                        BookingDate = DateTime.UtcNow
                    };
                    seatBooking.Seat = _context.Seats.FirstOrDefault(f => f.SeatId == seatBooking.SeatId);
                    _context.SeatBookings.Add(seatBooking);
                }

                // Xử lý vé chiều về (nếu có)
                if (checkoutData.ReturnFlight != null && checkoutData.SelectedReturnSeats != null)
                {
                    var returnAirplaneId = _context.Flights
                        .Include(f => f.Schedule)
                        .Where(f => f.FlightId == checkoutData.ReturnFlight.FlightId)
                        .Select(f => f.Schedule.AirplaneId)
                        .FirstOrDefault();
                    if (returnAirplaneId == null)
                        return BadRequest("Return airplane not found.");

                    foreach (var seatCode in checkoutData.SelectedReturnSeats)
                    {
                        var seat = _context.Seats.FirstOrDefault(
                            s => s.SeatNumber == seatCode && s.AirplaneId == returnAirplaneId);
                        if (seat == null) continue; 

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

                        tickets.Add(ticket);

                        var seatBooking = new SeatBooking
                        {
                            FlightId = ticket.FlightId,
                            SeatId = seat.SeatId,
                            AccountId = accountId,
                            IsBooked = true,
                            BookingDate = DateTime.UtcNow
                        };

                        _context.SeatBookings.Add(seatBooking);
                    }
                }

                // Lưu các vé vào DB
                _context.Tickets.AddRange(tickets);
                await _context.SaveChangesAsync();

                foreach (var ticket in tickets)
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
                    await _context.SaveChangesAsync(); // Lưu để lấy PaymentId

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

                // Xóa session
                HttpContext.Session.Remove("CheckoutData");

                return Ok(new { message = "Payment captured and booking completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Capture error: {ex.Message}");
                return StatusCode(500, "An error occurred while capturing the order.");
            }
        }



        #endregion
        //[HttpGet]
        //public IActionResult Payment(int flightId, int? returnFlightId, SeatSelectionViewModel Model)
        //{
        //    try
        //    {
        //        // Lấy thông tin chuyến bay
        //        var outboundFlight = _context.Flights
        //            .Include(f => f.Schedule)
        //            .ThenInclude(s => s.Route)
        //            .FirstOrDefault(f => f.FlightId == flightId);

        //        if (outboundFlight == null)
        //        {
        //            return NotFound("Không tìm thấy chuyến bay");
        //        }

        //        var outboundPrice = _context.Routes
        //            .Where(r => r.RouteId == outboundFlight.Schedule.RouteId)
        //            .Select(r => r.BasePrice)
        //            .FirstOrDefault();

        //        decimal totalAmount = outboundPrice * Model.SelectedSeatsOutBoard.Count;
        //        decimal returnPrice = 0;

        //        if (returnFlightId.HasValue)
        //        {
        //            var returnFlight = _context.Flights
        //                .Include(f => f.Schedule)
        //                .ThenInclude(s => s.Route)
        //                .FirstOrDefault(f => f.FlightId == returnFlightId);

        //            if (returnFlight != null)
        //            {
        //                returnPrice = _context.Routes
        //                    .Where(r => r.RouteId == returnFlight.Schedule.RouteId)
        //                    .Select(r => r.BasePrice)
        //                    .FirstOrDefault();

        //                totalAmount += returnPrice * Model.SelectedSeatsReturnBoard.Count;
        //            }
        //        }

        //        // Tạo view model
        //        var viewModel = new PaymentViewModel
        //        {
        //            OutboundFlightId = flightId,
        //            ReturnFlightId = returnFlightId,
        //            IsRoundTrip = returnFlightId.HasValue,
        //            PassengerCount = Model.SelectedSeatsOutBoard.Count,
        //            SelectedOutboundSeats = Model.SelectedSeatsOutBoard,
        //            SelectedReturnSeats = Model.SelectedSeatsReturnBoard,
        //            TotalAmount = totalAmount,
        //            TaxAmount = totalAmount * 0.1m, // VAT 10%

        //            // Khởi tạo thông tin hành khách
        //            Passengers = Enumerable.Range(0, Model.SelectedSeatsOutBoard.Count)
        //                .Select(_ => new PassengerInfo())
        //                .ToList()
        //        };

        //        return RedirectToAction("CreatePayPalOrder", "Payment", new
        //        {
        //            amount = viewModel.TotalAmount,
        //            currency = "USD",
        //            description = "Flight Booking Payment"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError(ex, "Lỗi khi tải trang thanh toán");
        //        return StatusCode(500, "Đã xảy ra lỗi khi tải trang thanh toán");
        //    }
        //}


    }


}
