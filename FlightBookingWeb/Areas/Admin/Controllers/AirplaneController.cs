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
            var airplanes = await _context.Airplanes.ToListAsync();
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
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Airplane/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AirplaneExists(int id)
        {
            return _context.Airplanes.Any(e => e.AirplaneId == id);
        }
    }
}
