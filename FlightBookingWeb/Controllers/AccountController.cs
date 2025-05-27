using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.ViewModels;
using FlightBookingWeb.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FlightBookingWeb.Service;

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
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
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
                if (Url.IsLocalUrl(ReturnUrl))
                    return Redirect(ReturnUrl);

                if (user.Role == "Employee")
                {
                    // Chuyển hướng đến trang quản lý nếu là Admin
                    return RedirectToAction("Index", "Home", new { area = "Employee" });
                }
                else if (user.Role == "Admin")
                {
                    // Chuyển hướng đến trang quản lý nếu là Admin
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
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
            return RedirectToAction("Index", "Home");
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
        //public IActionResult Profile()
        //{
        //    // Lấy thông tin người dùng hiện tại
        //    var username = User.Identity.Name;
        //    var user = _context.Accounts.FirstOrDefault(u => u.Username == username);
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }
        //    return View(user);
        //}

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

        //[HttpPost]
        //[Authorize] // Chỉ cho phép người dùng đã đăng nhập
        //public async Task<IActionResult> Profile(Account model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Lấy thông tin người dùng hiện tại
        //    var username = User.Identity.Name;
        //    var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    // Cập nhật thông tin cá nhân
        //    user.Email = model.Email;
        //    user.PhoneNumber = model.PhoneNumber;
        //    user.Gender = model.Gender;

        //    _context.Accounts.Update(user);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = "Your profile has been updated successfully.";
        //    return RedirectToAction(nameof(Profile));
        //}

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Profile()
        //{
        //    // Lấy userId từ claim
        //    var userIdStr = User.FindFirst("UserId")?.Value;
        //    if (!int.TryParse(userIdStr, out int userId))
        //        return NotFound("User not found.");

        //    // Lấy thông tin account
        //    var user = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountId == userId);
        //    if (user == null)
        //        return NotFound("User not found.");

        //    // Lấy lịch sử đặt vé (bao gồm chuyến bay, ghế, v.v.)
        //    var tickets = await _context.Tickets
        //        .Where(t => t.AccountId == userId)
        //        .Include(t => t.Flight)
        //            .ThenInclude(f => f.Schedule)
        //                .ThenInclude(s => s.Route)
        //        .Include(t => t.Seat)
        //        .Include(t => t.Payments)
        //        .Include(t => t.Baggages)
        //        .ToListAsync();

        //    // Truyền dữ liệu qua ViewBag
        //    ViewBag.Account = user;
        //    ViewBag.Tickets = tickets;

        //    return View();
        //}


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BookingHistory()
        {
            // Lấy userId từ claim
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return NotFound("Không tìm thấy người dùng.");

            var now = DateTime.UtcNow;

            // Lấy tất cả vé của user, bao gồm thông tin chuyến bay và ghế
            var tickets = await _context.Tickets
                .Where(t => t.AccountId == userId)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Schedule)
                .Include(t => t.Seat)
                .ToListAsync();

            // Vé đã bay (chuyến bay đã khởi hành)
            var pastTickets = tickets
                .Where(t => t.Flight.DepartureDateTime < now)
                .OrderByDescending(t => t.Flight.DepartureDateTime)
                .ToList();

            // Vé chưa bay (chuyến bay còn trong tương lai hoặc hôm nay)
            var futureTickets = tickets
                .Where(t => t.Flight.DepartureDateTime >= now)
                .OrderBy(t => t.Flight.DepartureDateTime)
                .ToList();

            ViewBag.PastTickets = pastTickets;
            ViewBag.FutureTickets = futureTickets;

            return View();
        }

        #region Update TicketSeat
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpgradeSeat(int ticketId)
        {
            // Lấy thông tin vé, chuyến bay, ghế hiện tại
            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Seat == null)
                return NotFound("Ticket or seat not found.");

            // Danh sách hạng ghế theo thứ tự
            var seatClassOrder = new List<string> { "Economy", "Business", "First" };
            var currentClassIndex = seatClassOrder.IndexOf(ticket.Seat.SeatClass);

            // Lấy các hạng ghế cao hơn hạng hiện tại
            var upgradeOptions = seatClassOrder
                .Where((c, idx) => idx > currentClassIndex)
                .ToList();

            // Truyền dữ liệu qua ViewBag
            ViewBag.Ticket = ticket;
            ViewBag.Flight = ticket.Flight;
            ViewBag.Seat = ticket.Seat;
            ViewBag.UpgradeOptions = upgradeOptions;

            return View();
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpgradeSeat(int ticketId, string targetSeatClass)
        {
            // Lấy thông tin vé và seat booking
            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Schedule)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.SeatId == null)
                return NotFound("Ticket or seat not found.");

            var seatBooking = await _context.SeatBookings
                .FirstOrDefaultAsync(sb => sb.FlightId == ticket.FlightId && sb.SeatId == ticket.SeatId);

            if (seatBooking == null)
                return NotFound("Seat booking not found.");

            // Xác định thứ tự hạng ghế
            var seatClassOrder = new List<string> { "Economy", "Business", "First" };
            var currentClassIndex = seatClassOrder.IndexOf(ticket.Seat.SeatClass);
            var targetClassIndex = seatClassOrder.IndexOf(targetSeatClass);

            // Chỉ cho nâng hạng (không cho hạ hạng hoặc giữ nguyên)
            if (targetClassIndex <= currentClassIndex)
                return BadRequest("You can only upgrade to a higher class.");

            // Tìm ghế trống thuộc hạng cao hơn trên cùng chuyến bay (máy bay)
            var airplaneId = ticket.Flight.Schedule.AirplaneId;
            var bookedSeatIds = await _context.SeatBookings
                .Where(sb => sb.FlightId == ticket.FlightId && sb.IsBooked)
                .Select(sb => sb.SeatId)
                .ToListAsync();

            var availableSeat = await _context.Seats
                .Where(s => s.AirplaneId == airplaneId
                    && s.SeatClass == targetSeatClass
                    && !bookedSeatIds.Contains(s.SeatId))
                .FirstOrDefaultAsync();

            if (availableSeat == null)
                return BadRequest("No available seat in the requested class.");

            // Thay đổi: Redirect sang trang checkout nâng hạng, truyền ticketId và targetSeatId
            return RedirectToAction("CheckoutUpgradeSeat", new { ticketId = ticket.TicketId, targetSeatId = availableSeat.SeatId });
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckoutUpgradeSeat(int ticketId, int targetSeatId)
        {
            // Lấy thông tin vé và ghế hiện tại
            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Schedule)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Seat == null)
                return NotFound("Ticket or seat not found.");

            // Lấy thông tin ghế sẽ đổi
            var targetSeat = await _context.Seats
                .FirstOrDefaultAsync(s => s.SeatId == targetSeatId);

            if (targetSeat == null)
                return NotFound("Target seat not found.");

            // Lấy giá base của route
            var basePrice = await _context.Routes
                .Where(r => r.RouteId == ticket.Flight.Schedule.RouteId)
                .Select(r => r.BasePrice)
                .FirstOrDefaultAsync();

            // Tính giá ghế hiện tại và ghế sẽ đổi
            var currentSeatPrice = SeatPricingHelper.CalculateSeatPrice(basePrice, ticket.Seat.SeatClass, ticket.Seat.SeatType);
            var targetSeatPrice = SeatPricingHelper.CalculateSeatPrice(basePrice, targetSeat.SeatClass, targetSeat.SeatType);
            var extraAmount = targetSeatPrice - currentSeatPrice;

            // Truyền dữ liệu qua ViewBag
            ViewBag.TicketId = ticket.TicketId;
            ViewBag.FlightId = ticket.FlightId;
            ViewBag.CurrentSeat = ticket.Seat;
            ViewBag.TargetSeat = targetSeat;
            ViewBag.CurrentSeatPrice = currentSeatPrice;
            ViewBag.TargetSeatPrice = targetSeatPrice;
            ViewBag.ExtraAmount = extraAmount;

            return View("CheckoutUpgradeSeat");
        }

        public class UpgradePayPalOrderRequest
        {
            public int TicketId { get; set; }
            public string TargetSeatClass { get; set; }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUpgradePayPalOrder([FromBody] UpgradePayPalOrderRequest request)
        {
            // Lấy thông tin vé và seat hiện tại
            int ticketId = request.TicketId;
            string targetSeatClass = request.TargetSeatClass;
            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Schedule)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Seat == null)
                return BadRequest("Ticket or seat not found.");

            // Lấy giá base của route
            var basePrice = await _context.Routes
                .Where(r => r.RouteId == ticket.Flight.Schedule.RouteId)
                .Select(r => r.BasePrice)
                .FirstOrDefaultAsync();

            // Tính giá ghế hiện tại và ghế muốn nâng
            var currentSeatPrice = FlightBookingWeb.Service.SeatPricingHelper.CalculateSeatPrice(basePrice, ticket.Seat.SeatClass, ticket.Seat.SeatType);

            // Tìm một ghế trống thuộc hạng muốn nâng
            var airplaneId = ticket.Flight.Schedule.AirplaneId;
            var bookedSeatIds = await _context.SeatBookings
                .Where(sb => sb.FlightId == ticket.FlightId && sb.IsBooked)
                .Select(sb => sb.SeatId)
                .ToListAsync();

            var targetSeat = await _context.Seats
                .Where(s => s.AirplaneId == airplaneId
                    && s.SeatClass == targetSeatClass
                    && !bookedSeatIds.Contains(s.SeatId))
                .FirstOrDefaultAsync();

            if (targetSeat == null)
                return BadRequest("No available seat in the requested class.");

            var targetSeatPrice = FlightBookingWeb.Service.SeatPricingHelper.CalculateSeatPrice(basePrice, targetSeat.SeatClass, targetSeat.SeatType);

            // Tính tiền bù thêm
            var extraAmount = targetSeatPrice - currentSeatPrice;
            if (extraAmount <= 0)
                return BadRequest("No upgrade fee required or invalid upgrade.");

            // Tạo order PayPal
            var currency = "USD";
            var description = $"Upgrade seat for ticket #{ticket.TicketId} to {targetSeatClass}";
            var orderId = await HttpContext.RequestServices.GetRequiredService<IPayPalService>()
                .CreateOrderAsync(extraAmount, currency, description);

            // Lưu thông tin upgrade tạm thời vào session để dùng khi capture
            HttpContext.Session.SetString("UpgradeSeatInfo", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                TicketId = ticketId,
                TargetSeatId = targetSeat.SeatId,
                ExtraAmount = extraAmount
            }));

            return Ok(new { id = orderId, amount = extraAmount });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CaptureUpgradePayPalOrder([FromQuery] string orderId)
        {
            // Capture thanh toán với PayPal
            var payPalService = HttpContext.RequestServices.GetRequiredService<IPayPalService>();
            var result = await payPalService.CapturePaymentAsync(orderId);
            if (!result)
                return BadRequest("Failed to capture PayPal order.");

            // Lấy thông tin upgrade từ session
            var upgradeInfoJson = HttpContext.Session.GetString("UpgradeSeatInfo");
            if (string.IsNullOrEmpty(upgradeInfoJson))
                return BadRequest("No upgrade info found in session.");

            dynamic upgradeInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(upgradeInfoJson);
            int ticketId = upgradeInfo.TicketId;
            int targetSeatId = upgradeInfo.TargetSeatId;
            decimal extraAmount = upgradeInfo.ExtraAmount;

            // Cập nhật lại seat cho ticket và seat booking
            var ticket = await _context.Tickets.Include(t => t.Flight).FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null)
                return NotFound("Ticket not found.");

            var seatBooking = await _context.SeatBookings
                .FirstOrDefaultAsync(sb => sb.FlightId == ticket.FlightId && sb.SeatId == ticket.SeatId);
            if (seatBooking == null)
                return NotFound("Seat booking not found.");

            ticket.SeatId = targetSeatId;
            seatBooking.SeatId = targetSeatId;
            _context.Tickets.Update(ticket);
            _context.SeatBookings.Update(seatBooking);

            // Lưu thông tin thanh toán bổ sung (có thể tạo bảng riêng hoặc lưu vào bảng Payment)
            var payment = new Payment
            {
                TicketId = ticket.TicketId,
                Amount = extraAmount,
                PaymentMethod = "Paypal Upgrade",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed",
                TransactionId = orderId
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            // Xóa session upgrade
            HttpContext.Session.Remove("UpgradeSeatInfo");

            TempData["SuccessMessage"] = "Seat upgraded and payment completed successfully.";
            return Ok(new { message = "Seat upgraded and payment completed successfully." });
        }
        #endregion

    }
}
