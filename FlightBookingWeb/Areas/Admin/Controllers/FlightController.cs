using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FlightController : Controller
    {
        private readonly AppDbContext _context;

        public FlightController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string search, DateTime? fromDate, DateTime? toDate, int? scheduleId)
        {
            var flightsQuery = _context.Flights
    .Where(f => f.Status != "Đã hủy")
    .Include(f => f.Schedule)
        .ThenInclude(s => s.Route)
            .ThenInclude(r => r.DepartureAirport)
    .Include(f => f.Schedule)
        .ThenInclude(s => s.Route)
            .ThenInclude(r => r.ArrivalAirport)
    .Include(f => f.Schedule) // thêm dòng này
        .ThenInclude(s => s.Airplane) // để lấy thông tin máy bay
    .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                flightsQuery = flightsQuery.Where(f =>
                    f.FlightId.ToString().Contains(search) ||
                    f.Schedule.Route.DepartureAirport.AirportName.Contains(search) ||
                    f.Schedule.Route.ArrivalAirport.AirportName.Contains(search) ||
                    f.Schedule.Airplane.AirplaneName.Contains(search));
                    
            }

            if (fromDate.HasValue)
            {
                flightsQuery = flightsQuery.Where(f => f.DepartureDateTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                flightsQuery = flightsQuery.Where(f => f.DepartureDateTime <= toDate.Value);
            }

            if (scheduleId.HasValue)
            {
                flightsQuery = flightsQuery.Where(f => f.ScheduleId == scheduleId.Value);
            }

            ViewBag.Schedules = new SelectList(_context.FlightSchedules, "ScheduleId", "ScheduleId");

            var flights = await flightsQuery.ToListAsync();
            return View(flights);
        }


        public IActionResult Edit(int id)
        {
            var flight = _context.Flights
                .Include(f => f.Schedule) // Include Schedule nếu cần trong View
                .FirstOrDefault(f => f.FlightId == id);

            if (flight == null)
            {
                return NotFound();
            }

            // Tìm lịch trình tương ứng với đầy đủ thông tin Route và Airport
            var schedule = _context.FlightSchedules
                .Include(s => s.Route)
                    .ThenInclude(r => r.DepartureAirport)
                .Include(s => s.Route)
                    .ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefault(s => s.ScheduleId == flight.ScheduleId);

            // Gán tên lịch trình nếu đầy đủ dữ liệu
            if (schedule?.Route?.DepartureAirport != null && schedule?.Route?.ArrivalAirport != null)
            {
                ViewBag.ScheduleName = schedule.Route.DepartureAirport.AirportName + " - " + schedule.Route.ArrivalAirport.AirportName;
            }
            else
            {
                ViewBag.ScheduleName = "Không rõ lịch trình";
            }

            // Đổ danh sách Schedule vào dropdown nếu cần chỉnh sửa
            ViewBag.Schedules = new SelectList(
                _context.FlightSchedules
                    .Where(fs => fs.Status && fs.Active)
                    .Include(fs => fs.Route)
                        .ThenInclude(r => r.DepartureAirport)
                    .Include(fs => fs.Route)
                        .ThenInclude(r => r.ArrivalAirport)
                    .ToList(),
                "ScheduleId",
                "ScheduleId", // Có thể thay bằng tên lịch trình chi tiết nếu cần
                flight.ScheduleId
            );

            return View(flight);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                return NotFound();
            }

            // Ghi log ModelState để kiểm tra các lỗi nếu có
            foreach (var state in ModelState)
            {
                Console.WriteLine($"{state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            var schedule = _context.FlightSchedules
                .Include(s => s.Route)
                .FirstOrDefault(s => s.ScheduleId == flight.ScheduleId);

            if (schedule == null)
            {
                ModelState.AddModelError("", "Không tìm thấy lịch trình.");
            }

            var khoiHanhMoi = flight.DepartureDateTime;
            var denMoi = flight.ArrivalDateTime;

            var today = DateTime.Today;
            var endDate = khoiHanhMoi.AddDays(60); // có thể thay đổi giới hạn nếu cần

            // Lấy tất cả chuyến bay có cùng máy bay với chuyến bay hiện tại và chưa bị hủy hoặc hoàn thành
            var flights = _context.Flights
                .Include(f => f.Schedule)
                .Where(f => f.Schedule.AirplaneId == schedule.AirplaneId  // Trùng mã máy bay
                    && f.FlightId != flight.FlightId  // Tránh chuyến bay hiện tại
                    && f.Status != "Đã hủy"  // Loại bỏ các chuyến bay đã bị hủy
                    && f.Status != "Đã hoàn thành" 
                    && f.Status != "Đang bay")  // Loại bỏ các chuyến bay đã hoàn thành
                .ToList();

            // Kiểm tra lịch trình mới có bị trùng với các chuyến bay khác không
            foreach (var f in flights)
            {
                var fStart = f.DepartureDateTime;
                var fEnd = f.ArrivalDateTime;

                // Kiểm tra nếu chuyến bay mới có lịch trình trùng với chuyến bay khác
                if (khoiHanhMoi < fEnd && denMoi > fStart)
                {
                    ModelState.AddModelError("", $"Lịch trình bị trùng với chuyến bay khác từ {fStart:dd/MM/yyyy HH:mm} đến {fEnd:dd/MM/yyyy HH:mm}.");
                    break;
                }
            }

            // Nếu không có lỗi trong ModelState, tiến hành cập nhật chuyến bay
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Quay lại danh sách chuyến bay
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Flights.Any(f => f.FlightId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu có lỗi, hiển thị lại thông tin chuyến bay và các lựa chọn lịch trình
            ViewBag.Schedules = new SelectList(_context.FlightSchedules.Where(fs => fs.Status && fs.Active), "ScheduleId", "ScheduleId", flight.ScheduleId);
            return View(flight);
        }



        public async Task<IActionResult> Details(int id)
        {
            // Tìm chuyến bay theo FlightId
            var flight = await _context.Flights
                .Include(f => f.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.DepartureAirport)
                .Include(f => f.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefaultAsync(f => f.FlightId == id);

            // Nếu không tìm thấy chuyến bay
            if (flight == null)
            {
                return NotFound();
            }

            // Trả về View cùng với dữ liệu chuyến bay
            return View(flight);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _context.Flights
    .Include(f => f.Schedule)
        .ThenInclude(s => s.Route)
            .ThenInclude(r => r.DepartureAirport)
    .Include(f => f.Schedule)
        .ThenInclude(s => s.Route)
            .ThenInclude(r => r.ArrivalAirport)
    .FirstOrDefaultAsync(f => f.FlightId == id);

            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flights.FindAsync(id);

            if (flight != null)
            {
                flight.Status = "Đã hủy"; // Chuyển trạng thái thay vì xóa
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }

            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách
        }

    }
}
