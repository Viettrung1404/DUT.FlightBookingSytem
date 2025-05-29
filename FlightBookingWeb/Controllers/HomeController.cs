using System.Diagnostics;
using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
