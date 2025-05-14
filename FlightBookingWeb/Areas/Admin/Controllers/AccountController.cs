using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using FlightBookingWeb.Areas.Admin.ViewModels;

namespace FlightBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Account
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Accounts
                .Where(a => a.Role != "Delete")
                .ToListAsync();

            var viewModels = accounts.Select(a => new AccountViewModel
            {
                AccountId = a.AccountId,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Gender = a.Gender,
                Role = a.Role
            }).ToList();

            return View(viewModels);
        }

        // GET: Admin/Account/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            var viewModel = new AccountViewModel
            {
                AccountId = account.AccountId,
                Username = account.Username,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Gender = account.Gender,
                Role = account.Role
            };

            return View(viewModel);
        }

        // GET: Admin/Account/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountViewModel account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra email trùng
                    if (await _context.Accounts.AnyAsync(a => a.Email == account.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View(account); // Hiển thị lại form với lỗi
                    }

                    // Kiểm tra username trùng
                    if (await _context.Accounts.AnyAsync(a => a.Username == account.Username))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                        return View(account);
                    }

                    var entity = new Account
                    {
                        Username = account.Username,
                        Password = BCrypt.Net.BCrypt.HashPassword(account.Password),
                        Email = account.Email,
                        PhoneNumber = account.PhoneNumber,
                        Gender = account.Gender,
                        Role = account.Role
                    };

                    _context.Accounts.Add(entity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Nếu vì lý do gì đó vẫn bị lỗi (tránh exception văng ra)
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu tài khoản.");
                    return View(account);
                }
            }

            return View(account); // Form không hợp lệ
        }

        // POST: Admin/Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountViewModel account)
        {
            if (id != account.AccountId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingAccount = await _context.Accounts.FindAsync(id);
                if (existingAccount == null)
                    return NotFound();

                existingAccount.Username = account.Username;
                existingAccount.Email = account.Email;
                existingAccount.PhoneNumber = account.PhoneNumber;
                existingAccount.Gender = account.Gender;
                existingAccount.Role = account.Role;

                _context.Update(existingAccount);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(account);
        }

        // GET: Admin/Account/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            var viewModel = new AccountViewModel
            {
                AccountId = account.AccountId,
                Username = account.Username,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Gender = account.Gender,
                Role = account.Role
            };

            return View(viewModel);
        }

        // POST: Admin/Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            account.Role = "Delete"; // Xóa mềm
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
