using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        public BookingController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Search()
        {
            return View(new FlightViewModel());
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
                            .Select(r => r.Price)
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
                                .Select(r => r.Price)
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
        public IActionResult Payment(int flightId, int? returnFlightId, List<string> selectedSeatsOutBoard, List<string> selectedSeatsReturnBoard)
        {
            try
            {
                // Lấy thông tin chuyến bay
                var outboundFlight = _context.Flights
                    .Include(f => f.Schedule)
                    .ThenInclude(s => s.Route)
                    .FirstOrDefault(f => f.FlightId == flightId);

                if (outboundFlight == null)
                {
                    return NotFound("Không tìm thấy chuyến bay");
                }

                // Tính toán giá tiền
                var outboundPrice = _context.Routes
                    .Where(r => r.RouteId == outboundFlight.Schedule.RouteId)
                    .Select(r => r.Price)
                    .FirstOrDefault();

                decimal totalAmount = outboundPrice * selectedSeatsOutBoard.Count;
                decimal returnPrice = 0;

                if (returnFlightId.HasValue)
                {
                    var returnFlight = _context.Flights
                        .Include(f => f.Schedule)
                        .ThenInclude(s => s.Route)
                        .FirstOrDefault(f => f.FlightId == returnFlightId);

                    if (returnFlight != null)
                    {
                        returnPrice = _context.Routes
                            .Where(r => r.RouteId == returnFlight.Schedule.RouteId)
                            .Select(r => r.Price)
                            .FirstOrDefault();

                        totalAmount += returnPrice * selectedSeatsReturnBoard.Count;
                    }
                }

                // Tạo view model
                var viewModel = new PaymentViewModel
                {
                    OutboundFlightId = flightId,
                    ReturnFlightId = returnFlightId,
                    IsRoundTrip = returnFlightId.HasValue,
                    PassengerCount = selectedSeatsOutBoard.Count,
                    SelectedOutboundSeats = selectedSeatsOutBoard,
                    SelectedReturnSeats = selectedSeatsReturnBoard,
                    TotalAmount = totalAmount,
                    TaxAmount = totalAmount * 0.1m, // VAT 10%

                    // Khởi tạo thông tin hành khách
                    Passengers = Enumerable.Range(0, selectedSeatsOutBoard.Count)
                        .Select(_ => new PassengerInfo())
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Lỗi khi tải trang thanh toán");
                return StatusCode(500, "Đã xảy ra lỗi khi tải trang thanh toán");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            try
            {
                // 1. Tạo vé (Ticket)
                var ticket = new Ticket
                {
                    BookingDate = DateTime.Now,
                    Status = "Confirmed",
                    // Thêm các thông tin khác nếu cần
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // 2. Tạo thanh toán (Payment)
                var payment = new Payment
                {
                    TicketId = ticket.TicketId,
                    Amount = model.GrandTotal,
                    PaymentMethod = model.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Completed",
                    TransactionId = Guid.NewGuid().ToString()
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 3. Tạo hóa đơn (Invoice)
                var invoice = new Invoice
                {
                    PaymentId = payment.PaymentId,
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{payment.PaymentId}",
                    IssueDate = DateTime.Now,
                    TaxAmount = model.TaxAmount,
                    TotalAmount = model.TotalAmount
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // 4. Tạo chi tiết vé (TicketDetail) cho từng ghế
                foreach (var seatNumber in model.SelectedOutboundSeats)
                {
                    var ticketDetail = new Ticket
                    {
                        TicketId = ticket.TicketId,
                        FlightId = model.OutboundFlightId,
                        // Thêm thông tin hành khách tương ứng
                        PassengerName = model.Passengers[model.SelectedOutboundSeats.IndexOf(seatNumber)].FullName,
                        PassengerEmail = model.Passengers[model.SelectedOutboundSeats.IndexOf(seatNumber)].Email
                    };
                    _context.TicketDetails.Add(ticketDetail);
                }

                if (model.IsRoundTrip)
                {
                    foreach (var seatNumber in model.SelectedReturnSeats)
                    {
                        var ticketDetail = new TicketDetail
                        {
                            TicketId = ticket.TicketId,
                            FlightId = model.ReturnFlightId.Value,
                            SeatNumber = seatNumber,
                            // Thêm thông tin hành khách tương ứng
                            PassengerName = model.Passengers[model.SelectedReturnSeats.IndexOf(seatNumber)].FullName,
                            PassengerEmail = model.Passengers[model.SelectedReturnSeats.IndexOf(seatNumber)].Email
                        };
                        _context.TicketDetails.Add(ticketDetail);
                    }
                }

                await _context.SaveChangesAsync();

                // 5. Chuyển đến trang xác nhận
                var confirmationModel = new PaymentConfirmationViewModel
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    IssueDate = invoice.IssueDate.Value,
                    TotalAmount = invoice.TotalAmount,
                    TaxAmount = invoice.TaxAmount,
                    GrandTotal = invoice.TotalAmount + invoice.TaxAmount,
                    TransactionId = payment.TransactionId,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    OutboundFlightId = model.OutboundFlightId,
                    ReturnFlightId = model.ReturnFlightId,
                    OutboundSeats = model.SelectedOutboundSeats,
                    ReturnSeats = model.SelectedReturnSeats
                };

                return View("PaymentConfirmation", confirmationModel);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Lỗi khi xử lý thanh toán");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại.");
                return View("Payment", model);
            }
        }


    }


}
