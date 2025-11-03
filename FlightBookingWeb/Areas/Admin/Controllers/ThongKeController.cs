using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Doanh thu theo tháng
            var doanhThuTheoThang = await _context.Invoices
                .Where(i => i.IssueDate != null)
                .GroupBy(i => new { i.IssueDate.Value.Year, i.IssueDate.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    ThangNam = $"{g.Key.Month:D2}/{g.Key.Year}",
                    TongTien = g.Sum(x => x.TotalAmount),
                    Thue = g.Sum(x => x.TaxAmount),
                    TongHoaDon = g.Count()
                })
                .ToListAsync();

            // Lưu lượng hành khách theo tháng (mỗi Ticket là 1 hành khách)
            var luuLuongHanhKhach = await _context.Payments
                .Where(p => p.PaymentDate != null)
                .ToListAsync();

            var hanhKhachTheoThang = luuLuongHanhKhach
                .GroupBy(p => new { p.PaymentDate.Value.Year, p.PaymentDate.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    ThangNam = $"{g.Key.Month:D2}/{g.Key.Year}",
                    SoLuongHanhKhach = g.Count() // 👈 Đúng tên View mong đợi
                })
                .ToList();

            ViewBag.DoanhThu = doanhThuTheoThang;
            ViewBag.LuuLuong = hanhKhachTheoThang;

            return View();
        }
    }
}
