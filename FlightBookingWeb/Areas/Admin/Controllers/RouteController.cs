using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RouteController : Controller
    {
        private readonly AppDbContext _context;

        public RouteController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var routes = await _context.Routes
            .Where(r => r.Status == "Active") // Chỉ lấy những cái chưa bị xóa
            .Select(r => new RouteViewModel
            {
                RouteId = r.RouteId,
                DepartureAirportId = r.DepartureAirportId,
                DepartureAirportName = _context.Airports.Where(s => s.AirportId == r.DepartureAirportId).Select(s => s.AirportName).FirstOrDefault(),
                ArrivalAirportId = r.ArrivalAirportId,
                ArrivalAirportName = _context.Airports.Where(s => s.AirportId == r.ArrivalAirportId).Select(s => s.AirportName).FirstOrDefault(),
                Duration = r.Duration.ToString("HH:mm"),
                BasePrice = r.BasePrice
            })
            .ToListAsync();

            return View(routes);
        }

        public IActionResult Create()
        {
            ViewBag.Airports = new SelectList(_context.Airports, "AirportId", "AirportName");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RouteViewModel model)
        {
            TimeOnly duration = default; // Khởi tạo giá trị mặc định cho 'duration'
            ViewBag.Airports = new SelectList(_context.Airports, "AirportId", "AirportName");

            if (model.DepartureAirportId == model.ArrivalAirportId)
            {
                ModelState.AddModelError("DepartureAirportId", "San bay di va san bay den khong duoc trung nhau.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var route = new Models.Route
            {
                DepartureAirportId = model.DepartureAirportId,
                ArrivalAirportId = model.ArrivalAirportId,
                Duration = duration,
                BasePrice = model.BasePrice
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tuyen bay da duoc luu thanh cong.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            var model = new RouteViewModel
            {
                RouteId = route.RouteId,
                DepartureAirportId = route.DepartureAirportId,
                ArrivalAirportId = route.ArrivalAirportId,
                Duration = route.Duration.ToString("HH:mm"),  // Chuyển đổi Duration thành string để hiển thị
                BasePrice = route.BasePrice
            };

            ViewBag.Airports = new SelectList(_context.Airports, "AirportId", "AirportName");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RouteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(modelError.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                var route = await _context.Routes.FindAsync(model.RouteId);
                if (route == null) return NotFound();

                // Chuyển đổi Duration từ string thành TimeOnly
                route.DepartureAirportId = model.DepartureAirportId;
                route.ArrivalAirportId = model.ArrivalAirportId;
                route.Duration = TimeOnly.Parse(model.Duration);  // Chuyển đổi Duration từ string thành TimeOnly
                route.BasePrice = model.BasePrice;

                _context.Routes.Update(route);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tuyến bay đã được cập nhật thành công.";
                return RedirectToAction("Index");
            }

            // Nếu không hợp lệ, trả lại danh sách sân bay để người dùng chọn lại
            ViewBag.Airports = new SelectList(_context.Airports, "AirportId", "AirportName");
            return View(model);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                route.Status = "Delete"; // Đánh dấu đã xóa
                _context.Routes.Update(route);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}