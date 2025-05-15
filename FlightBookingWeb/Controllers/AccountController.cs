using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.ViewModels;
using FlightBookingWeb.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tìm người dùng theo Username
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                // Tạo Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("UserId", user.AccountId.ToString())
                };

                // Tạo ClaimsIdentity
                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                // Tạo Cookie
                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if(user.Role == "Employee")
                {
                    // Chuyển hướng đến trang quản lý nếu là Admin
                    return RedirectToAction("Index", "Home", new { area = "Employee" });
                }
                else if (user.Role == "Admin")
                {
                    // Chuyển hướng đến trang quản lý nếu là Admin
                    return RedirectToAction("Index", "Route", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("Index", "Home");
                }
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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                Username = model.Username,
                Password = hashedPassword,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Gender = model.Gender,
                Role = "Admin" // Gán vai trò mặc định
            };

            _context.Accounts.Add(newUser);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang đăng nhập sau khi đăng ký thành công
            return RedirectToAction("Login", "Account");
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "Account");
        }


        // GET: Index (Trang chủ của AccountController)
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize] // Chỉ cho phép người dùng đã đăng nhập
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize] // Chỉ cho phép người dùng đã đăng nhập
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại
            var username = User.Identity.Name;
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View(model);
            }

            // Mã hóa mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Index", "Home");
        }

        //[HttpGet]
        //[Authorize] // Chỉ cho phép người dùng đã đăng nhập
        //public async Task<IActionResult> Profile()
        //{
        //    // Lấy thông tin người dùng hiện tại
        //    var username = User.Identity.Name;
        //    var user = await _context.Accounts
        //        .Include(a => a.Invoices) // Bao gồm hóa đơn
        //        .ThenInclude(i => i.Ticket) // Bao gồm vé trong hóa đơn
        //        .ThenInclude(t => t.Flight) // Bao gồm thông tin chuyến bay
        //        .ThenInclude(f => f.Route) // Bao gồm thông tin tuyến bay
        //        .ThenInclude(r => r.DepartureAirport) // Bao gồm sân bay khởi hành
        //        .Include(a => a.Invoices)
        //        .ThenInclude(i => i.Ticket.Flight.Route.ArrivalAirport) // Bao gồm sân bay đến
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    return View(user);
        //}

        [HttpPost]
        [Authorize] // Chỉ cho phép người dùng đã đăng nhập
        public async Task<IActionResult> Profile(Account model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại
            var username = User.Identity.Name;
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Cập nhật thông tin cá nhân
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;

            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Profile));
        }



    }
}
