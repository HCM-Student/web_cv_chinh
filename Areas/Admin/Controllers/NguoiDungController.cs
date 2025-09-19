using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.Account;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : Controller
    {
        private readonly NewsDbContext _context;
        private readonly IPasswordHasher<NguoiDung> _passwordHasher;

        public NguoiDungController(NewsDbContext context, IPasswordHasher<NguoiDung> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: Admin/NguoiDung
        public async Task<IActionResult> Index()
        {
            var nguoiDungs = await _context.NguoiDungs.OrderBy(x => x.Id).ToListAsync();
            return View(nguoiDungs);
        }


        // GET: Admin/NguoiDung/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/NguoiDung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiDungCreateVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng lặp
                var existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(model);
                }

                // Tạo người dùng mới
                var nguoiDung = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email.ToLower(),
                    VaiTro = model.VaiTro,
                    KichHoat = model.KichHoat,
                    NgayTao = DateTime.Now
                };

                // Hash mật khẩu
                nguoiDung.MatKhauHash = _passwordHasher.HashPassword(nguoiDung, model.MatKhau);

                try
                {
                    _context.Add(nguoiDung);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Đã tạo người dùng '{model.HoTen}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Xử lý lỗi database
                    if (ex.InnerException?.Message.Contains("duplicate key") == true)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng. Vui lòng thử lại.");
                    }
                }
            }

            return View(model);
        }

        // GET: Admin/NguoiDung/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            var model = new NguoiDungEditVM
            {
                Id = nguoiDung.Id,
                HoTen = nguoiDung.HoTen,
                Email = nguoiDung.Email,
                VaiTro = nguoiDung.VaiTro,
                KichHoat = nguoiDung.KichHoat,
                NgayTao = nguoiDung.NgayTao
            };

            return View(model);
        }

        // POST: Admin/NguoiDung/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguoiDungEditVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng lặp (ngoại trừ user hiện tại)
                var existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Id != id);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(model);
                }

                try
                {
                    var nguoiDung = await _context.NguoiDungs.FindAsync(id);
                    if (nguoiDung == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin
                    nguoiDung.HoTen = model.HoTen;
                    nguoiDung.Email = model.Email.ToLower();
                    nguoiDung.VaiTro = model.VaiTro;
                    nguoiDung.KichHoat = model.KichHoat;

                    // Chỉ cập nhật mật khẩu nếu có nhập mật khẩu mới
                    if (!string.IsNullOrEmpty(model.MatKhauMoi))
                    {
                        nguoiDung.MatKhauHash = _passwordHasher.HashPassword(nguoiDung, model.MatKhauMoi);
                    }

                    _context.Update(nguoiDung);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Đã cập nhật người dùng '{model.HoTen}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoiDungExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("duplicate key") == true)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật người dùng. Vui lòng thử lại.");
                    }
                }
            }
            return View(model);
        }

        // GET: Admin/NguoiDung/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }

        // POST: Admin/NguoiDung/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung != null)
            {
                _context.NguoiDungs.Remove(nguoiDung);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/NguoiDung/ViewProfile/5
        public async Task<IActionResult> ViewProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .Include(n => n.BaiViets)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (nguoiDung == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                Id = nguoiDung.Id,
                HoTen = nguoiDung.HoTen,
                Email = nguoiDung.Email,
                VaiTro = nguoiDung.VaiTro,
                KichHoat = nguoiDung.KichHoat,
                NgayTao = nguoiDung.NgayTao,
                Avatar = nguoiDung.Avatar,
                SoBaiViet = nguoiDung.BaiViets.Count
            };

            return View(model);
        }

        // POST: Admin/NguoiDung/DeleteUser/5 (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(id);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Kiểm tra xem có phải admin không
                if (nguoiDung.VaiTro == "Admin")
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản Admin" });
                }

                _context.NguoiDungs.Remove(nguoiDung);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa người dùng '{nguoiDung.HoTen}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa người dùng: {ex.Message}" });
            }
        }

        // POST: Admin/NguoiDung/CleanupUsers (Xóa tài khoản test)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupUsers()
        {
            try
            {
                // Xóa tất cả tài khoản không phải Admin
                var nonAdminUsers = await _context.NguoiDungs
                    .Where(u => u.VaiTro != "Admin")
                    .ToListAsync();

                var adminCount = await _context.NguoiDungs
                    .CountAsync(u => u.VaiTro == "Admin");

                if (nonAdminUsers.Any())
                {
                    _context.NguoiDungs.RemoveRange(nonAdminUsers);
                    await _context.SaveChangesAsync();
                }

                return Json(new { 
                    success = true, 
                    message = $"Đã xóa {nonAdminUsers.Count} tài khoản test. Giữ lại {adminCount} tài khoản Admin." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi dọn dẹp: {ex.Message}" });
            }
        }

        // GET: Admin/NguoiDung/ForceCleanup (Xóa tài khoản test - không cần xác nhận)
        [HttpGet]
        public async Task<IActionResult> ForceCleanup()
        {
            try
            {
                // Xóa tất cả tài khoản không phải Admin
                var nonAdminUsers = await _context.NguoiDungs
                    .Where(u => u.VaiTro != "Admin")
                    .ToListAsync();

                var adminCount = await _context.NguoiDungs
                    .CountAsync(u => u.VaiTro == "Admin");

                if (nonAdminUsers.Any())
                {
                    _context.NguoiDungs.RemoveRange(nonAdminUsers);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Đã xóa {nonAdminUsers.Count} tài khoản test. Giữ lại {adminCount} tài khoản Admin.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi dọn dẹp: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool NguoiDungExists(int id)
        {
            return _context.NguoiDungs.Any(e => e.Id == id);
        }
    }

    // ViewModel for User Profile
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public bool KichHoat { get; set; }
        public DateTime NgayTao { get; set; }
        public string? Avatar { get; set; }
        public int SoBaiViet { get; set; }
    }
}
