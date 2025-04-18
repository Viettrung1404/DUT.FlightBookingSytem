using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.ViewModels;
using FlightBookingWeb.Models; 


namespace FlightBookingWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            bool exists = await _context.Accounts.AnyAsync(u => u.Username == model.Username);
            if (exists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            var newUser = new Account
            {
                Username = model.Username,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Gender = model.Gender,
                Role = "User" // Default role
            };

            _context.Accounts.Add(newUser);
            await _context.SaveChangesAsync();



            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}