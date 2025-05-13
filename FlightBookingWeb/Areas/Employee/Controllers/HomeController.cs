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
            var model = new EmployeeSearchIndexViewModel
            {
                Tickets = _context.Tickets.Include(t => t.Account).Select(t => new SearchViewModel
                {
                    TicketId = t.TicketId,
                    FlightID = t.FlightId,
                    Username = t.Account.Username,
                    BookingDate = t.BookingDate,
                    Price = t.Price,
                    Status = t.Status
                }).ToList(),

                Accounts = _context.Accounts.Select(a => new SearchViewModel
                {
                    AccountId = a.AccountId,
                    Username = a.Username,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    Role = a.Role
                }).ToList()
            };

            return View(model);
        }
        public async Task<IActionResult> SearchTickets(string? ticketKeyword)
        {
            var query = _context.Tickets.Include(t => t.Account).Include(t => t.Flight).AsQueryable();

            if (!string.IsNullOrEmpty(ticketKeyword))
            {
                if (int.TryParse(ticketKeyword, out int flightId))
                    query = query.Where(t => t.Flight.FlightId == flightId);
                else if (DateTime.TryParse(ticketKeyword, out DateTime date))
                    query = query.Where(t => t.BookingDate != null && t.BookingDate.Value.Date == date.Date);
                else
                    query = query.Where(t =>
                        EF.Functions.Like(t.Status!, $"%{ticketKeyword}%") ||
                        EF.Functions.Like(t.Account.Username, $"%{ticketKeyword}%"));
            }

            var result = await query.Select(t => new SearchViewModel
            {
                TicketId = t.TicketId,
                Status = t.Status,
                BookingDate = t.BookingDate,
                Price = t.Price,
                FlightID = t.Flight.FlightId,
                AccountId = t.Account.AccountId,
                Username = t.Account.Username
            }).ToListAsync();

            return PartialView("_TicketSearch", result);
        }

        public async Task<IActionResult> SearchAccounts(string? accountKeyword)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(accountKeyword))
            {
                query = query.Where(a =>
                    a.Username.Contains(accountKeyword) ||
                    a.Email.Contains(accountKeyword) ||
                    a.PhoneNumber!.Contains(accountKeyword));
            }

            var result = await query.Select(a => new SearchViewModel
            {
                AccountId = a.AccountId,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Role = a.Role
            }).ToListAsync();

            return PartialView("_AccountSearch", result);
        }

    }
}