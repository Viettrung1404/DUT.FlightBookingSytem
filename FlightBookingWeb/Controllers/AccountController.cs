using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.ViewModels;
using FlightBookingWeb.Models;
using BCrypt.Net;

namespace FlightBookingWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tìm người dùng theo Username
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                // Lưu thông tin người dùng vào session
                HttpContext.Session.SetString("UserId", user.AccountId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role ?? "User");

                // Chuyển hướng đến trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Thông báo lỗi nếu đăng nhập thất bại
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        private async Task<string> GenerateAccountIdAsync()
        {
            var lastAccount = await _context.Accounts
                .OrderByDescending(a => a.AccountId)
                .FirstOrDefaultAsync();

            if (lastAccount == null)
            {
                return "ACC0001"; // Giá trị khởi đầu
            }

            // Tăng giá trị ID
            int lastIdNumber = int.Parse(lastAccount.AccountId.Substring(3));
            return $"ACC{(lastIdNumber + 1):D4}";
        }


        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra mật khẩu có khớp không
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            // Kiểm tra xem username đã tồn tại chưa
            bool usernameExists = await _context.Accounts.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            // Kiểm tra xem email đã tồn tại chưa
            bool emailExists = await _context.Accounts.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View(model);
            }

            // Mã hóa mật khẩu
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Tạo tài khoản mới
            var newUser = new Account
            {
                AccountId = await GenerateAccountIdAsync(),
                Username = model.Username,
                Password = hashedPassword,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Gender = model.Gender,
                Role = "User" // Gán vai trò mặc định
            };

            _context.Accounts.Add(newUser);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang đăng nhập sau khi đăng ký thành công
            return RedirectToAction("Login", "Account");
        }

        // GET: Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // GET: Index (Trang chủ của AccountController)
        public IActionResult Index()
        {
            return View();
        }
    }
}
