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
            // Truy van ve
            var ticketQuery = _context.Tickets
                .Include(t => t.Account)
                .Include(t => t.Flight)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ticketKeyword))
            {
                ticketQuery = ticketQuery.Where(t =>
                    t.Status!.Contains(ticketKeyword) ||
                    t.Flight.FlightId==int.Parse(ticketKeyword) ||
                    t.Account.Username.Contains(ticketKeyword));
            }

            var ticketResults = await ticketQuery.Select(t => new SearchViewModel
            {
                TicketId = t.TicketId,
                Status = t.Status,
                BookingDate = t.BookingDate,
                Price = t.Price,
                FlightName = t.Flight.FlightId.ToString(),

                AccountId = t.Account.AccountId,
                Username = t.Account.Username,
                Email = t.Account.Email,
                PhoneNumber = t.Account.PhoneNumber,
                Role = t.Account.Role
            }).ToListAsync();

            // Truy van tai khoan
            var accountQuery = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(accountKeyword))
            {
                accountQuery = accountQuery.Where(a =>
                    a.Role == "User" && // Chỉ tìm kiếm tài khoản người dùng
                    (a.Username.Contains(accountKeyword) ||
                    a.Email.Contains(accountKeyword) ||
                    a.PhoneNumber!.Contains(accountKeyword)));
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
