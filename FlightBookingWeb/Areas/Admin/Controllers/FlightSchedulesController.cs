using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FlightSchedulesController : Controller
    {
        private readonly AppDbContext _context;

        public FlightSchedulesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string search, bool? active, string scheduleId)
        {
            var schedulesQuery = _context.FlightSchedules
                .Include(fs => fs.Airplane)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.DepartureAirport)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.ArrivalAirport)
                .Where(fs => fs.Status) // chỉ lấy các bản ghi có Status == true
                .AsQueryable();

            // Tạo dropdown danh sách Schedule
            var allSchedules = await _context.FlightSchedules.ToListAsync();
            ViewBag.Schedules = new SelectList(allSchedules, "ScheduleId", "ScheduleId");
            ViewBag.SelectedScheduleId = scheduleId;

            // Lọc tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                schedulesQuery = schedulesQuery.Where(fs =>
                    fs.ScheduleId.ToString().Contains(search) ||
                    fs.Airplane.AirplaneName.Contains(search) ||
                    fs.Route.DepartureAirport.AirportName.Contains(search) ||
                    fs.Route.ArrivalAirport.AirportName.Contains(search));
            }

            if (active.HasValue)
            {
                schedulesQuery = schedulesQuery.Where(fs => fs.Active == active.Value);
            }

            if (!string.IsNullOrEmpty(scheduleId))
            {
                schedulesQuery = schedulesQuery.Where(fs => fs.ScheduleId.ToString() == scheduleId);
            }

            var schedules = await schedulesQuery.ToListAsync();
            return View(schedules);
        }

        public IActionResult Details(int id)
        {
            var flightSchedule = _context.FlightSchedules
                .Include(fs => fs.Airplane)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.DepartureAirport)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefault(fs => fs.ScheduleId == id);

            if (flightSchedule == null)
                return NotFound();

            if (!flightSchedule.Status)
            {
                TempData["Message"] = "Lịch trình này đã bị hủy.";
                return RedirectToAction(nameof(Index));
            }

            return View(flightSchedule);
        }

        public IActionResult Create()
        {
            // Lấy tất cả các máy bay
            var availableAirplanes = _context.Airplanes
                .Select(a => new { a.AirplaneId, a.AirplaneName })
                .ToList();

            ViewBag.Airplanes = new SelectList(availableAirplanes, "AirplaneId", "AirplaneName");

            var routes = _context.Routes
                .Include(r => r.DepartureAirport)
                .Include(r => r.ArrivalAirport)
                .Where(r => r.Status == "Active")
                .Select(r => new
                {
                    r.RouteId,
                    Name = r.DepartureAirport.AirportName + " → " + r.ArrivalAirport.AirportName
                }).ToList();

            if (routes.Any())
            {
                ViewData["Routes"] = new SelectList(routes, "RouteId", "Name");
            }
            else
            {
                ModelState.AddModelError("RouteId", "Không có tuyến bay nào để chọn.");
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FlightSchedule flightSchedule)
        {
            // Kiểm tra giá trị của flightSchedule
            Console.WriteLine($"RouteId: {flightSchedule.RouteId}, AirplaneId: {flightSchedule.AirplaneId}, DepartureTime: {flightSchedule.DepartureTime}");

            var route = _context.Routes
                .Include(r => r.DepartureAirport)
                .Include(r => r.ArrivalAirport)
                .FirstOrDefault(r => r.RouteId == flightSchedule.RouteId);

            if (route == null)
            {
                ModelState.AddModelError("RouteId", "Tuyến bay không hợp lệ.");
            }

            var airplane = _context.Airplanes.FirstOrDefault(a => a.AirplaneId == flightSchedule.AirplaneId);

            if (airplane == null)
            {
                ModelState.AddModelError("AirplaneId", "Máy bay không hợp lệ.");
            }
            flightSchedule.Route = route;
            if (ModelState.IsValid)
            {
                // Assign the ArrivalTime based on the route's duration
                flightSchedule.ArrivalTime = route.Duration;

                // Check for schedule conflicts
                if (KiemTraTrungLich(flightSchedule))
                {
                    ModelState.AddModelError("", "Lịch trình bị trùng với chuyến bay khác sử dụng cùng máy bay.");
                    goto ReloadDropdowns;
                }

                _context.FlightSchedules.Add(flightSchedule);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

        ReloadDropdowns:
            var allRoutes = _context.Routes
                .Include(r => r.DepartureAirport)
                .Include(r => r.ArrivalAirport)
                .Select(r => new
                {
                    r.RouteId,
                    Name = r.DepartureAirport.AirportName + " → " + r.ArrivalAirport.AirportName
                }).ToList();

            ViewBag.Routes = new SelectList(allRoutes, "RouteId", "Name", flightSchedule.RouteId);
            ViewBag.Airplanes = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneName", flightSchedule.AirplaneId);

            return View(flightSchedule);
        }

        public IActionResult Edit(int id)
        {
            var flightSchedule = _context.FlightSchedules.Find(id);
            if (flightSchedule == null)
                return NotFound();

            LoadDropdowns(flightSchedule);
            return View(flightSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FlightSchedule flightSchedule, bool updateFlights = false)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(flightSchedule);
                return View(flightSchedule);
            }

            var route = _context.Routes.FirstOrDefault(r => r.RouteId == flightSchedule.RouteId);
            if (route == null)
            {
                ModelState.AddModelError("RouteId", "Tuyến bay không hợp lệ.");
                LoadDropdowns(flightSchedule);
                return View(flightSchedule);
            }

            var oldSchedule = _context.FlightSchedules.AsNoTracking().FirstOrDefault(fs => fs.ScheduleId == flightSchedule.ScheduleId);
            if (oldSchedule == null)
                return NotFound();

            flightSchedule.ArrivalTime = route.Duration;

            // 👉 Tự dời DepartureTime nếu updateFlights == false và có Frequency > 0
            if (flightSchedule.Frequency > 0 && !updateFlights && oldSchedule.DepartureTime != flightSchedule.DepartureTime)
            {
                var freqDays = flightSchedule.Frequency;
                var oldTime = flightSchedule.DepartureTime;
                var nowtime = DateTime.Now;
                var minTarget = nowtime.AddDays(60);

                int n = (int)Math.Ceiling((minTarget - oldTime).TotalDays / freqDays);
                flightSchedule.DepartureTime = oldTime.AddDays(n * freqDays);
            }

            if (KiemTraTrungLich(flightSchedule))
            {
                ModelState.AddModelError("", "Lịch trình bị trùng với chuyến bay khác sử dụng cùng máy bay.");
                LoadDropdowns(flightSchedule);
                return View(flightSchedule);
            }

            _context.Update(flightSchedule);
            _context.SaveChanges();

            // 👉 Nếu người dùng muốn cập nhật chuyến bay con
            if (updateFlights && oldSchedule.DepartureTime != flightSchedule.DepartureTime)
            {
                var flights = _context.Flights
                    .Where(f => f.ScheduleId == flightSchedule.ScheduleId
                             && f.DepartureDateTime > DateTime.Now
                             && f.Status == "Chưa cất cánh")
                    .OrderBy(f => f.DepartureDateTime)
                    .ToList();

                DateTime newDeparture = flightSchedule.DepartureTime;
                TimeSpan duration = flightSchedule.ArrivalTime.ToTimeSpan();

                if (flightSchedule.Frequency == 0)
                {
                    // Không lặp: chỉ cập nhật chuyến đầu tiên nếu có
                    if (flights.Count > 0)
                    {
                        flights[0].DepartureDateTime = newDeparture;
                        flights[0].ArrivalDateTime = newDeparture + duration;
                    }
                }
                else
                {
                    // Lặp: cập nhật toàn bộ
                    TimeSpan freq = TimeSpan.FromDays(flightSchedule.Frequency);
                    for (int i = 0; i < flights.Count; i++)
                    {
                        flights[i].DepartureDateTime = newDeparture + freq * i;
                        flights[i].ArrivalDateTime = flights[i].DepartureDateTime + duration;
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }


        private void LoadDropdowns(FlightSchedule flightSchedule)
        {
            ViewBag.Airplanes = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneName", flightSchedule.AirplaneId);

            var routes = _context.Routes
                .Include(r => r.DepartureAirport)
                .Include(r => r.ArrivalAirport)
                .Select(r => new
                {
                    r.RouteId,
                    Name = r.DepartureAirport.AirportName + " → " + r.ArrivalAirport.AirportName
                }).ToList();

            ViewBag.Routes = new SelectList(routes, "RouteId", "Name", flightSchedule.RouteId);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = _context.FlightSchedules
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.DepartureAirport)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.ArrivalAirport)
                .Include(fs => fs.Airplane)
                .FirstOrDefault(fs => fs.ScheduleId == id);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool cancelFlights)
        {
            var schedule = await _context.FlightSchedules.FindAsync(id);
            if (schedule != null)
            {
                if (cancelFlights)
                {
                    var flights = _context.Flights.Where(f => f.ScheduleId == id).ToList();
                    foreach (var flight in flights)
                    {
                        flight.Status = "Đã hủy";
                    }
                }

                schedule.Status = false; // Ngưng hoạt động lịch bay
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool KiemTraTrungLich(FlightSchedule newSchedule)
        {
            var lichTrinhs = _context.FlightSchedules
                .Where(fs => fs.AirplaneId == newSchedule.AirplaneId && fs.ScheduleId != newSchedule.ScheduleId&& fs.Status==true &&fs.Active==true)
                .ToList();

            var today = DateTime.Now;
            var endDate = today.AddDays(60);

            var khoiHanhMoi = newSchedule.DepartureTime;
            var denMoi = khoiHanhMoi + newSchedule.ArrivalTime.ToTimeSpan();

            // 🔁 Kiểm tra trùng với các lịch trình khác
            foreach (var lich in lichTrinhs)
            {
                var khoiHanhCu = lich.DepartureTime;
                var denCu = khoiHanhCu + lich.ArrivalTime.ToTimeSpan();

                if (newSchedule.Frequency == 0 && lich.Frequency == 0)
                {
                    // Cả hai không lặp
                    if (!(denMoi <= khoiHanhCu || denCu <= khoiHanhMoi))
                        return true;
                }
                else
                {
                    // Lặp: tạo danh sách tất cả thời điểm có thể xảy ra
                    var timesMoi = new List<(DateTime start, DateTime end)>();
                    var timesCu = new List<(DateTime start, DateTime end)>();

                    if (newSchedule.Frequency == 0)
                    {
                        timesMoi.Add((khoiHanhMoi, denMoi));
                    }
                    else
                    {
                        DateTime temp = khoiHanhMoi;
                        while (temp <= endDate)
                        {
                            var end = temp + newSchedule.ArrivalTime.ToTimeSpan();
                            if (end >= today)
                                timesMoi.Add((temp, end));
                            temp = temp.AddDays(newSchedule.Frequency);
                        }
                    }

                    if (lich.Frequency == 0)
                    {
                        timesCu.Add((khoiHanhCu, denCu));
                    }
                    else
                    {
                        DateTime temp = khoiHanhCu;
                        while (temp <= endDate)
                        {
                            var end = temp + lich.ArrivalTime.ToTimeSpan();
                            if (end >= today)
                                timesCu.Add((temp, end));
                            temp = temp.AddDays(lich.Frequency);
                        }
                    }

                    foreach (var moi in timesMoi)
                    {
                        foreach (var cu in timesCu)
                        {
                            if (!(moi.end <= cu.start || cu.end <= moi.start))
                                return true;
                        }
                    }
                }
            }

            // 🛫 Kiểm tra trùng với các chuyến bay đã được tạo
            var flights = _context.Flights
    .Where(f => f.Schedule.AirplaneId == newSchedule.AirplaneId
             && f.ScheduleId != newSchedule.ScheduleId  // ✅ Khác lịch trình
             && f.Status != "Đã hủy")
    .ToList();

            var listMoi = new List<(DateTime start, DateTime end)>();
            if (newSchedule.Frequency == 0)
            {
                listMoi.Add((khoiHanhMoi, denMoi));
            }
            else
            {
                DateTime temp = khoiHanhMoi;
                while (temp <= endDate)
                {
                    var end = temp + newSchedule.ArrivalTime.ToTimeSpan();
                    if (end >= today)
                        listMoi.Add((temp, end));
                    temp = temp.AddDays(newSchedule.Frequency);
                }
            }

            foreach (var f in flights)
            {
                var fStart = f.DepartureDateTime;
                var fEnd = f.ArrivalDateTime;

                foreach (var moi in listMoi)
                {
                    if (!(moi.end <= fStart || fEnd <= moi.start))
                        return true;
                }
            }

            return false;
        }



    }
}