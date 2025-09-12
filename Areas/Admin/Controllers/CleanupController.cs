using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CleanupController : Controller
    {
        private readonly NewsDbContext _context;

        public CleanupController(NewsDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Cleanup/RemoveTestAccounts
        public async Task<IActionResult> RemoveTestAccounts()
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
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi dọn dẹp: {ex.Message}";
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}


