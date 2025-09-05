using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BaiVietController : Controller
    {
        private readonly NewsDbContext _db;
        public BaiVietController(NewsDbContext db) => _db = db;

        // GET: /Admin/BaiViet
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .OrderByDescending(x => x.NgayDang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x =>
                    x.TieuDe.Contains(q) ||
                    (x.TomTat ?? "").Contains(q) ||
                    (x.ChuyenMuc != null && x.ChuyenMuc.Ten.Contains(q)) ||
                    (x.TacGia != null && x.TacGia.HoTen.Contains(q))
                );
            }

            ViewBag.Q = q;
            return View(await query.ToListAsync());
        }

        // GET: /Admin/BaiViet/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new BaiViet());
        }

        // GET: /Admin/BaiViet/CreateEvent
        public async Task<IActionResult> CreateEvent()
        {
            await LoadDropdowns();
            // Tìm chuyên mục "Sự kiện" (nếu có) để chọn sẵn
            var suKien = await _db.ChuyenMucs.FirstOrDefaultAsync(x => x.Ten == "Sự kiện");
            var model = new BaiViet
            {
                ChuyenMucId = suKien?.Id ?? 0,
                NgayDang = DateTime.UtcNow
            };
            ViewData["PresetCategoryName"] = suKien?.Ten ?? "";
            return View("Create", model);
        }

        // POST: /Admin/BaiViet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang")] BaiViet model)
        {
            // Bỏ validate cho navigation properties
            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            // Validate chọn dropdown
            if (model.ChuyenMucId <= 0)
                ModelState.AddModelError(nameof(model.ChuyenMucId), "Vui lòng chọn chuyên mục.");
            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            if (model.NgayDang == default)
                model.NgayDang = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            try
            {
                _db.BaiViets.Add(model);
                await _db.SaveChangesAsync();
                TempData["msg"] = "Đã thêm bài viết.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, $"Lưu thất bại: {ex.InnerException?.Message ?? ex.Message}");
                await LoadDropdowns();
                return View(model);
            }
        }

        // GET: /Admin/BaiViet/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            await LoadDropdowns();
            return View(bv);
        }

        // POST: /Admin/BaiViet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang")] BaiViet model)
        {
            if (id != model.Id) return BadRequest();

            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            if (model.ChuyenMucId <= 0)
                ModelState.AddModelError(nameof(model.ChuyenMucId), "Vui lòng chọn chuyên mục.");
            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            bv.TieuDe = model.TieuDe;
            bv.TomTat = model.TomTat;
            bv.NoiDung = model.NoiDung;
            bv.ChuyenMucId = model.ChuyenMucId;
            bv.TacGiaId = model.TacGiaId;
            bv.NgayDang = model.NgayDang;

            try
            {
                await _db.SaveChangesAsync();
                TempData["msg"] = "Đã cập nhật bài viết.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, $"Cập nhật thất bại: {ex.InnerException?.Message ?? ex.Message}");
                await LoadDropdowns();
                return View(model);
            }
        }

        // GET: /Admin/BaiViet/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var bv = await _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bv == null) return NotFound();
            return View(bv);
        }

        // POST: /Admin/BaiViet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            try
            {
                _db.BaiViets.Remove(bv);
                await _db.SaveChangesAsync();
                TempData["msg"] = "Đã xoá bài viết.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["msg"] = $"Không thể xoá: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadDropdowns()
        {
            ViewBag.ChuyenMucId = new SelectList(
                await _db.ChuyenMucs.OrderBy(x => x.Ten).ToListAsync(), "Id", "Ten");

            ViewBag.TacGiaId = new SelectList(
                await _db.NguoiDungs.OrderBy(x => x.HoTen).ToListAsync(), "Id", "HoTen");
        }
    }
}
