using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using FlightBookingWeb.Areas.Admin.ViewModels;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AirplaneController : Controller
    {
        private readonly AppDbContext _context;

        public AirplaneController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Airplane
        public async Task<IActionResult> Index()
        {
            var airplanes = await _context.Airplanes.Where(a => a.Status == "Active").ToListAsync();
            List<AirplaneViewModel> model = new List<AirplaneViewModel>();
            foreach (var item in airplanes) {
                model.Add(new AirplaneViewModel
                {
                    AirplaneId = item.AirplaneId,
                    AirplaneName = item.AirplaneName,
                    AirplaneType = item.AirplaneType,
                    TotalSeats = item.TotalSeats,
                    EconomySeats = item.EconomySeats,
                    BusinessSeats = item.BusinessSeats,
                    ManufactureYear = item.ManufactureYear,
                    Status = item.Status
                });
            }
            return View(model);
        }

        // GET: Admin/Airplane/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!AirplaneExists(id.Value))
            {
                return NotFound();
            }
            var airplane = await _context.Airplanes
                .FirstOrDefaultAsync(m => m.AirplaneId == id);
            if (airplane == null)
            {
                return NotFound();
            }

            return View(new AirplaneViewModel
            {
                AirplaneId = airplane.AirplaneId,
                AirplaneName = airplane.AirplaneName,
                AirplaneType = airplane.AirplaneType,
                TotalSeats = airplane.TotalSeats,
                EconomySeats = airplane.EconomySeats,
                BusinessSeats = airplane.BusinessSeats,
                ManufactureYear = airplane.ManufactureYear,
                Status = airplane.Status
            });
        }

        // GET: Admin/Airplane/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirplaneViewModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(new Airplane
                {
                    AirplaneName = model.AirplaneName,
                    AirplaneType = model.AirplaneType,
                    TotalSeats = model.TotalSeats,
                    EconomySeats = model.EconomySeats,
                    BusinessSeats = model.BusinessSeats,
                    ManufactureYear = model.ManufactureYear,
                    Status = model.Status
                });

                // Lấy ID của máy bay vừa tạo
                await _context.SaveChangesAsync();
                var airplane = await _context.Airplanes
                    .OrderByDescending(a => a.AirplaneId)
                    .FirstOrDefaultAsync();
                CreateSeat(airplane.AirplaneId, model.EconomySeats, model.BusinessSeats);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Airplane/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Tìm máy bay theo ID và chưa bị xóa
            if (!AirplaneExists(id.Value))
            {
                return NotFound();
            }
            var airplane = await _context.Airplanes.FindAsync(id);
            if (airplane == null)
            {
                return NotFound();
            }
            return View(new AirplaneViewModel
            {
                AirplaneId = airplane.AirplaneId,
                AirplaneName = airplane.AirplaneName,
                AirplaneType = airplane.AirplaneType,
                TotalSeats = airplane.TotalSeats,
                EconomySeats = airplane.EconomySeats,
                BusinessSeats = airplane.BusinessSeats,
                ManufactureYear = airplane.ManufactureYear,
                Status = airplane.Status
            });
        }

        // POST: Admin/Airplane/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AirplaneViewModel model)
        {
            if (id != model.AirplaneId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingAirplane = await _context.Airplanes.FindAsync(id);
                if (existingAirplane == null) return NotFound();

                if (existingAirplane.Status != "Active")
                {
                    ModelState.AddModelError("", "Máy bay đã bị xóa, không thể cập nhật.");
                    return View(model);
                }

                // Cập nhật thuộc tính
                existingAirplane.AirplaneName = model.AirplaneName;
                existingAirplane.AirplaneType = model.AirplaneType;
                existingAirplane.TotalSeats = model.TotalSeats;
                existingAirplane.EconomySeats = model.EconomySeats;
                existingAirplane.BusinessSeats = model.BusinessSeats;
                existingAirplane.ManufactureYear = model.ManufactureYear;
                existingAirplane.Status = model.Status;

                try
                {
                    _context.Update(existingAirplane);
                    CreateSeat(id, model.EconomySeats, model.BusinessSeats);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AirplaneExists(model.AirplaneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Airplane/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!AirplaneExists(id.Value))
            {
                return NotFound();
            }
            var airplane = await _context.Airplanes
                .FirstOrDefaultAsync(m => m.AirplaneId == id);
            if (airplane == null)
            {
                return NotFound();
            }

            return View(new AirplaneViewModel
            {
                AirplaneId = airplane.AirplaneId,
                AirplaneName = airplane.AirplaneName,
                AirplaneType = airplane.AirplaneType,
                TotalSeats = airplane.TotalSeats,
                EconomySeats = airplane.EconomySeats,
                BusinessSeats = airplane.BusinessSeats,
                ManufactureYear = airplane.ManufactureYear,
                Status = airplane.Status
            });
        }

        // POST: Admin/Airplane/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplane = await _context.Airplanes.FindAsync(id);
            if (airplane != null)
            {
                airplane.Status = "Deleted";
                _context.Update(airplane);
            }
            DeleteSeats(id);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AirplaneExists(int id)
        {
            return _context.Airplanes.Any(e => e.AirplaneId == id && e.Status == "Active");
        }

        private void CreateSeat(int airplaneId, int economy, int business)
        {
            DeleteSeats(airplaneId);
            // Khởi tạo ghế phổ 
            int seatsPerRow = 6;
            char[] seatLetters = { 'A', 'B', 'C', 'D', 'E', 'F' };

            for (int i = 0; i < economy; i++)
            {
                int row = (i / seatsPerRow) + 1;
                char seatLetter = seatLetters[i % seatsPerRow];

                var seat = new Seat
                {
                    AirplaneId = airplaneId,
                    SeatNumber = $"A{seatLetter}{row}",
                    SeatClass = "Economy",
                    SeatType = GetSeatType(seatLetter)
                };
                _context.Seats.Add(seat);
            }

            // Khởi tạo ghế thương gia
            for (int i = 0; i < business; i++)
            {
                int row = (i / seatsPerRow) + 1;
                char seatLetter = seatLetters[i % seatsPerRow];

                var seat = new Seat
                {
                    AirplaneId = airplaneId,
                    SeatNumber = $"B{seatLetter}{row}",
                    SeatClass = "Business",
                    SeatType = GetSeatType(seatLetter)
                };
                _context.Seats.Add(seat);
            }
        }

        private void DeleteSeats(int airplaneId)
        {
            // Xóa tất cả ghế của máy bay
            var seats = _context.Seats.Where(s => s.AirplaneId == airplaneId).ToList();
            if (seats.Count > 0)
            {
                _context.Seats.RemoveRange(seats);
            }
        }

        private string GetSeatType(char seatLetter)
        {
            // Mỗi hàng có 6 ghế: A, B, C, D, E, F.
            // A, F là ghế cửa sổ; B, E là ghế giữa; C, D là ghế lối đi.
            switch (seatLetter)
            {
                case 'A':
                case 'F':
                    return "Window";
                case 'C':
                case 'D':
                    return "Aisle";
                case 'B':
                case 'E':
                    return "Middle";
                default:
                    return "Middle";
            }
        }
    }
}
