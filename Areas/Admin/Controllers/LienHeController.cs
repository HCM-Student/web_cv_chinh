using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class LienHeController : Controller
    {
        private readonly NewsDbContext _context;

        public LienHeController(NewsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lienHes = await _context.LienHes
                .AsNoTracking()
                .OrderByDescending(lh => lh.NgayGui)
                .ToListAsync();

            return View(lienHes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lienHe = await _context.LienHes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lienHe == null) return NotFound();
            return View(lienHe);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lienHe = await _context.LienHes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lienHe == null) return NotFound();
            return View(lienHe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe != null)
            {
                _context.LienHes.Remove(lienHe);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string trangThai, string? ghiChu = null)
        {
            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe == null) return NotFound();

            lienHe.TrangThai = trangThai;
            if (!string.IsNullOrEmpty(ghiChu))
            {
                lienHe.GhiChu = ghiChu;
            }
            
            // Tự động cập nhật DaXuLy dựa trên trạng thái
            lienHe.DaXuLy = trangThai == "Đã xử lý" || trangThai == "Hoàn thành";

            await _context.SaveChangesAsync();
            TempData["msg"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Reply(int id)
        {
            var lienHe = _context.LienHes.Find(id);
            if (lienHe == null) return NotFound();

            // Tạo mailto link với thông tin đã điền sẵn
            var subject = $"Re: {lienHe.TieuDe}";
            var body = $"\n\n--- Tin nhắn gốc ---\nTừ: {lienHe.HoTen} ({lienHe.Email})\nNgày: {lienHe.NgayGui:dd/MM/yyyy HH:mm}\nNội dung:\n{lienHe.NoiDung}";
            
            var mailtoUrl = $"mailto:{lienHe.Email}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
            
            return Redirect(mailtoUrl);
        }
    }
}
