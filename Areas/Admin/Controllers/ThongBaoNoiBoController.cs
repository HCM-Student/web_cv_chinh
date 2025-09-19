using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Areas.Admin.ViewModels;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien,Staff")]
    public class ThongBaoNoiBoController : Controller
    {
        private readonly NewsDbContext _db;
        public ThongBaoNoiBoController(NewsDbContext db) => _db = db;

        // GET: /Admin/ThongBaoNoiBo
        public async Task<IActionResult> Index([FromQuery] InternalNoticeFilterVm f)
        {
            var q = _db.InternalNotices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(f.q))
            {
                var kw = f.q.Trim();
                q = q.Where(x => EF.Functions.Like(x.Title, $"%{kw}%")
                              || EF.Functions.Like(x.Summary ?? "", $"%{kw}%")
                              || EF.Functions.Like(x.Body, $"%{kw}%"));
            }
            if (f.from.HasValue)
            {
                var fromUtc = f.from.Value.Date.ToUniversalTime();
                q = q.Where(x => x.PublishAt >= fromUtc);
            }
            if (f.to.HasValue)
            {
                var toUtc = f.to.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                q = q.Where(x => x.PublishAt <= toUtc);
            }
            if (f.onlyActive == true)
                q = q.Where(x => x.IsActive);

            f.Total = await q.CountAsync();

            // order: Ghim trước -> mới nhất
            q = q.OrderByDescending(x => x.IsPinned)
                 .ThenByDescending(x => x.PublishAt);

            var skip = Math.Max(0, (f.page - 1) * Math.Max(1, f.ps));
            f.Items = await q.Skip(skip).Take(Math.Max(1, f.ps)).ToListAsync();
            return View(f);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View("Edit", new InternalNotice
            {
                PublishAt = DateTime.UtcNow
            });
        }

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InternalNotice m)
        {
            if (!ModelState.IsValid) return View("Edit", m);
            m.CreatedAt = m.UpdatedAt = DateTime.UtcNow;
            m.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            m.CreatedByName = User.FindFirstValue(ClaimTypes.Name);
            _db.InternalNotices.Add(m);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã tạo thông báo.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.InternalNotices.FindAsync(id);
            if (m == null) return NotFound();
            return View(m);
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InternalNotice m)
        {
            if (id != m.Id) return BadRequest();
            if (!ModelState.IsValid) return View(m);

            var dbm = await _db.InternalNotices.FindAsync(id);
            if (dbm == null) return NotFound();

            dbm.Title = m.Title;
            dbm.Summary = m.Summary;
            dbm.Body = m.Body;
            dbm.PublishAt = m.PublishAt;
            dbm.ExpireAt = m.ExpireAt;
            dbm.IsActive = m.IsActive;
            dbm.IsPinned = m.IsPinned;
            dbm.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.InternalNotices.FindAsync(id);
            if (m == null) return NotFound();
            _db.InternalNotices.Remove(m);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã xóa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var m = await _db.InternalNotices.FindAsync(id);
            if (m == null) return NotFound();
            m.IsActive = !m.IsActive;
            m.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var m = await _db.InternalNotices.FindAsync(id);
            if (m == null) return NotFound();
            m.IsPinned = !m.IsPinned;
            m.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
