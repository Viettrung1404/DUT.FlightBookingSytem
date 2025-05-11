using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? ticketKeyword, string? accountKeyword)
        {
            // --- TICKET SEARCH ---
            var ticketQuery = _context.Tickets
                .Include(t => t.Account)
                .Include(t => t.Flight)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ticketKeyword))
            {
                // Check if keyword is a number (FlightId)
                if (int.TryParse(ticketKeyword, out int flightId))
                {
                    ticketQuery = ticketQuery.Where(t => t.Flight.FlightId == flightId);
                }
                // Check if keyword is a valid DateTime
                else if (DateTime.TryParse(ticketKeyword, out DateTime bookingDate))
                {
                    ticketQuery = ticketQuery.Where(t => t.BookingDate != null &&
                        t.BookingDate.Value.Date == bookingDate.Date);
                }
                else
                {
                    // Search by Status or Username (case-insensitive)
                    ticketQuery = ticketQuery.Where(t =>
                        EF.Functions.Like(t.Status!, $"%{ticketKeyword}%") ||
                        EF.Functions.Like(t.Account.Username, $"%{ticketKeyword}%"));
                }
            }

            var ticketResults = await ticketQuery.Select(t => new SearchViewModel
            {
                TicketId = t.TicketId,
                Status = t.Status,
                BookingDate = t.BookingDate,
                Price = t.Price,
                FlightID = t.Flight.FlightId, // Or t.Flight.FlightCode if available
                AccountId = t.Account.AccountId,
                Username = t.Account.Username,
                Email = t.Account.Email,
                PhoneNumber = t.Account.PhoneNumber,
                Role = t.Account.Role
            }).ToListAsync();

            // --- USER SEARCH (Role = "User" only) ---
            var accountQuery = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(accountKeyword))
            {
                accountQuery = accountQuery.Where(a =>
                    a.Username.Contains(accountKeyword) ||
                    a.Email.Contains(accountKeyword) ||
                    a.PhoneNumber!.Contains(accountKeyword));
            }

            var accountResults = await accountQuery.Select(a => new SearchViewModel
            {
                AccountId = a.AccountId,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Role = a.Role
            }).ToListAsync();

            ViewBag.Tickets = ticketResults;
            ViewBag.Accounts = accountResults;

            return View();
        }
    }
}