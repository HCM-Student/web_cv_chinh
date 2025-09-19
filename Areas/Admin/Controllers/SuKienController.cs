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
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class SuKienController : Controller
    {
        private readonly NewsDbContext _db;
        public SuKienController(NewsDbContext db) => _db = db;

        private async Task<ChuyenMuc> GetOrCreateEventCategoryAsync()
        {
            var cat = await _db.ChuyenMucs.FirstOrDefaultAsync(x => x.Ten == "Sự kiện");
            if (cat == null)
            {
                cat = new ChuyenMuc { Ten = "Sự kiện", Slug = "su-kien" };
                _db.ChuyenMucs.Add(cat);
                await _db.SaveChangesAsync();
            }
            return cat;
        }

        // GET: /Admin/SuKien
        public async Task<IActionResult> Index(string? q)
        {
            var cat = await GetOrCreateEventCategoryAsync();
            var query = _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .Where(x => x.ChuyenMucId == cat.Id)
                .OrderByDescending(x => x.NgayDang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x => x.TieuDe.Contains(q) || (x.TomTat ?? "").Contains(q));
            }

            ViewBag.Q = q;
            return View(await query.ToListAsync());
        }

        // GET: /Admin/SuKien/Create
        public async Task<IActionResult> Create()
        {
            var cat = await GetOrCreateEventCategoryAsync();
            await LoadDropdowns(preselectedCategoryId: cat.Id);
            return View(new BaiViet { ChuyenMucId = cat.Id, NgayDang = DateTime.UtcNow });
        }

        // POST: /Admin/SuKien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TieuDe,TomTat,NoiDung,TacGiaId,NgayDang")] BaiViet model)
        {
            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            var cat = await GetOrCreateEventCategoryAsync();
            model.ChuyenMucId = cat.Id;
            if (model.NgayDang == default) model.NgayDang = DateTime.UtcNow;

            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(preselectedCategoryId: cat.Id);
                return View(model);
            }

            _db.BaiViets.Add(model);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã thêm sự kiện.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/SuKien/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await GetOrCreateEventCategoryAsync();
            var bv = await _db.BaiViets.FirstOrDefaultAsync(x => x.Id == id && x.ChuyenMucId == cat.Id);
            if (bv == null) return NotFound();

            await LoadDropdowns(preselectedCategoryId: cat.Id);
            return View(bv);
        }

        // POST: /Admin/SuKien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TieuDe,TomTat,NoiDung,TacGiaId,NgayDang")] BaiViet model)
        {
            var cat = await GetOrCreateEventCategoryAsync();
            var bv = await _db.BaiViets.FirstOrDefaultAsync(x => x.Id == id && x.ChuyenMucId == cat.Id);
            if (bv == null) return NotFound();

            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(preselectedCategoryId: cat.Id);
                return View(model);
            }

            bv.TieuDe = model.TieuDe;
            bv.TomTat = model.TomTat;
            bv.NoiDung = model.NoiDung;
            bv.TacGiaId = model.TacGiaId;
            bv.NgayDang = model.NgayDang;

            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã cập nhật sự kiện.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/SuKien/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await GetOrCreateEventCategoryAsync();
            var bv = await _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .FirstOrDefaultAsync(x => x.Id == id && x.ChuyenMucId == cat.Id);
            if (bv == null) return NotFound();
            return View(bv);
        }

        // POST: /Admin/SuKien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await GetOrCreateEventCategoryAsync();
            var bv = await _db.BaiViets.FirstOrDefaultAsync(x => x.Id == id && x.ChuyenMucId == cat.Id);
            if (bv == null) return NotFound();

            _db.BaiViets.Remove(bv);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã xoá sự kiện.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? preselectedCategoryId = null)
        {
            ViewBag.ChuyenMucId = new SelectList(
                await _db.ChuyenMucs.OrderBy(x => x.Ten).ToListAsync(), "Id", "Ten", preselectedCategoryId);

            ViewBag.TacGiaId = new SelectList(
                await _db.NguoiDungs.OrderBy(x => x.HoTen).ToListAsync(), "Id", "HoTen");
        }
    }
}


