using Microsoft.AspNetCore.Mvc;
using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.ViewModels;
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

        // GET: Booking/Search
        public IActionResult Search()
        {
            return View(new FlightSearchViewModel());
        }

        // POST: Booking/SearchResults
        [HttpPost]
        public IActionResult SearchResults(FlightSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Search", model);
            }

            var flights = _context.Flights
                .Include(f => f.Route)
                .ThenInclude(r => r.DepartureAirport)
                .Include(f => f.Route)
                .ThenInclude(r => r.ArrivalAirport)
                .Where(f =>
                    f.Route.DepartureAirport.AirportName.Contains(model.DepartureAirport) &&
                    f.Route.ArrivalAirport.AirportName.Contains(model.ArrivalAirport) &&
                    (!model.DepartureDate.HasValue || f.DepartureTime.Value.Date == model.DepartureDate.Value.Date))
                .Select(f => new FlightViewModel
                {
                    FlightId = f.FlightId,
                    DepartureAirport = f.Route.DepartureAirport.AirportName,
                    ArrivalAirport = f.Route.ArrivalAirport.AirportName,
                    DepartureTime = f.DepartureTime.Value,
                    ArrivalTime = f.ArrivalTime.Value,
                    Price = 100.00m // Giá vé mặc định
                })
                .ToList();

            return View(flights);
        }

        // GET: Booking/Book
        public IActionResult Book(int flightId)
        {
            var flight = _context.Flights
                .Include(f => f.Route)
                .ThenInclude(r => r.DepartureAirport)
                .Include(f => f.Route)
                .ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                return NotFound("Flight not found.");
            }

            var viewModel = new FlightBookingViewModel
            {
                FlightId = flightId
            };

            return View(viewModel);
        }

        // POST: Booking/Book
        [HttpPost]
        public IActionResult Book(FlightBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["BookingInfo"] = model;
            return RedirectToAction("Payment", new { flightId = model.FlightId });
        }

        // GET: Booking/Payment
        public IActionResult Payment(int flightId)
        {
            var bookingInfo = TempData["BookingInfo"] as FlightBookingViewModel;
            if (bookingInfo == null)
            {
                TempData["Error"] = "Booking information is missing.";
                return RedirectToAction("Search");
            }

            return View(bookingInfo);
        }

        // POST: Booking/ConfirmPayment
        [HttpPost]
        public IActionResult ConfirmPayment(FlightBookingViewModel model)
        {
            // Lưu thông tin đặt vé vào cơ sở dữ liệu
            foreach (var passenger in model.Passengers)
            {
                var ticket = new Ticket
                {
                    FlightId = model.FlightId,
                    SeatPosition = "Auto-Assigned", // Ghế được gán tự động
                    Price = 100.00m, // Giá vé mặc định
                    Customer = new Account
                    {
                        Username = passenger.Name,
                        Email = passenger.Email,
                        PhoneNumber = passenger.PhoneNumber
                    },
                    Class = new TicketClass
                    {
                        ClassName = model.TicketClass
                    }
                };

                _context.Tickets.Add(ticket);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment confirmed and tickets booked successfully!";
            return RedirectToAction("Confirmation", new { flightId = model.FlightId });
        }

        // GET: Booking/Confirmation
        public IActionResult Confirmation(int flightId)
        {
            var flight = _context.Flights
                .Include(f => f.Route)
                .ThenInclude(r => r.DepartureAirport)
                .Include(f => f.Route)
                .ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                TempData["Error"] = "Flight not found.";
                return RedirectToAction("Search");
            }

            return View(flight);
        }
    }
}
