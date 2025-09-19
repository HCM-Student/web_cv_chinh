using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class BinhLuanController : Controller
    {
        private readonly NewsDbContext _context;

        public BinhLuanController(NewsDbContext context)
        {
            _context = context;
        }

        // GET: Admin/BinhLuan
        public async Task<IActionResult> Index(int? trangThai)
        {
            var query = _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Include(b => b.BaiViet)
                .AsQueryable();

            // Calculate stats
            var allComments = await _context.BinhLuans.ToListAsync();
            ViewBag.TotalComments = allComments.Count;
            ViewBag.PendingComments = allComments.Count(c => c.TrangThai == 0);
            ViewBag.ApprovedComments = allComments.Count(c => c.TrangThai == 1);
            ViewBag.RejectedComments = allComments.Count(c => c.TrangThai == 2);
            ViewBag.TodayComments = allComments.Count(c => c.Ngay.Date == DateTime.Today);

            if (trangThai.HasValue)
            {
                query = query.Where(b => b.TrangThai == trangThai.Value);
            }

            var comments = await query
                .OrderBy(b => b.Id)
                .ToListAsync();

            ViewBag.TrangThai = trangThai;
            return View(comments);
        }

        // GET: Admin/BinhLuan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var binhLuan = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Include(b => b.BaiViet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (binhLuan == null)
            {
                return NotFound();
            }

            return View(binhLuan);
        }

        // POST: Admin/BinhLuan/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan == null)
            {
                return NotFound();
            }

            binhLuan.TrangThai = 1; // Đã duyệt
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bình luận đã được phê duyệt!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BinhLuan/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan == null)
            {
                return NotFound();
            }

            binhLuan.TrangThai = 2; // Từ chối
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bình luận đã bị từ chối!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BinhLuan/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan != null)
            {
                _context.BinhLuans.Remove(binhLuan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bình luận đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BinhLuan/BulkApprove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApprove(int[] commentIds)
        {
            if (commentIds != null && commentIds.Length > 0)
            {
                var comments = await _context.BinhLuans
                    .Where(b => commentIds.Contains(b.Id))
                    .ToListAsync();

                foreach (var comment in comments)
                {
                    comment.TrangThai = 1; // Đã duyệt
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã phê duyệt {comments.Count} bình luận!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BinhLuan/BulkReject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkReject(int[] commentIds)
        {
            if (commentIds != null && commentIds.Length > 0)
            {
                var comments = await _context.BinhLuans
                    .Where(b => commentIds.Contains(b.Id))
                    .ToListAsync();

                foreach (var comment in comments)
                {
                    comment.TrangThai = 2; // Từ chối
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã từ chối {comments.Count} bình luận!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
