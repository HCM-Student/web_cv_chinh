using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;               // IFormFile
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
        private readonly IWebHostEnvironment _env;

        public BaiVietController(NewsDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ==== Cấu hình thư mục lưu ảnh tiêu đề ====
        private string ThumbRoot => Path.Combine(_env.WebRootPath, "media", "posts", "cover");
        private static readonly string[] allowedImg = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg" };

        private async Task<string> SaveThumbAsync(IFormFile f)
        {
            Directory.CreateDirectory(ThumbRoot);

            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!allowedImg.Contains(ext)) throw new Exception("Định dạng ảnh không hỗ trợ.");

            var baseName = Path.GetFileNameWithoutExtension(f.FileName).Trim();
            // làm sạch tên file
            var safe = string.Concat(baseName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch)).ToLower();
            if (string.IsNullOrWhiteSpace(safe)) safe = "thumb";

            var fileName = $"{safe}{ext}";
            var dst = Path.Combine(ThumbRoot, fileName);

            int i = 1;
            while (System.IO.File.Exists(dst))
            {
                fileName = $"{safe}-{i++}{ext}";
                dst = Path.Combine(ThumbRoot, fileName);
            }

            await using var s = System.IO.File.Create(dst);
            await f.CopyToAsync(s);

            // trả về URL public
            return $"/media/posts/cover/{fileName}";
        }

        private void TryDeletePhysical(string? publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl)) return;
            var rel = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(_env.WebRootPath, rel);
            if (System.IO.File.Exists(full))
            {
                try { System.IO.File.Delete(full); } catch { /* ignore */ }
            }
        }

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
        public async Task<IActionResult> Create(
            [Bind("TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang,AnhTieuDeAlt")] BaiViet model,
            IFormFile? AnhTieuDeFile)
        {
            // bỏ validate navigation
            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

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

            // Lưu ảnh tiêu đề (nếu có file)
            if (AnhTieuDeFile?.Length > 0)
            {
                try { model.AnhTieuDe = await SaveThumbAsync(AnhTieuDeFile); }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload ảnh lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
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
        [HttpGet]
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
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang,AnhTieuDeAlt")] BaiViet model,
            IFormFile? AnhTieuDeFile, bool? removeThumb)
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

            bv.TieuDe       = model.TieuDe;
            bv.TomTat       = model.TomTat;
            bv.NoiDung      = model.NoiDung;
            bv.ChuyenMucId  = model.ChuyenMucId;
            bv.TacGiaId     = model.TacGiaId;
            bv.NgayDang     = model.NgayDang;
            bv.AnhTieuDeAlt = model.AnhTieuDeAlt;

            // xử lý ảnh: xoá / thay / giữ nguyên
            if (removeThumb == true && !string.IsNullOrEmpty(bv.AnhTieuDe))
            {
                TryDeletePhysical(bv.AnhTieuDe);
                bv.AnhTieuDe = null;
            }
            else if (AnhTieuDeFile?.Length > 0)
            {
                // thay ảnh mới -> xoá file cũ (nếu có), rồi lưu mới
                if (!string.IsNullOrEmpty(bv.AnhTieuDe))
                    TryDeletePhysical(bv.AnhTieuDe);

                try { bv.AnhTieuDe = await SaveThumbAsync(AnhTieuDeFile); }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload ảnh lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
            }

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
            // GET: /Admin/BaiViet/Details/5
            [HttpGet]
            public async Task<IActionResult> Details(int id)
            {
                var bv = await _db.BaiViets
                    .Include(x => x.ChuyenMuc)
                    .Include(x => x.TacGia)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (bv == null) return NotFound();
                return View(bv);
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
                // xoá file ảnh kèm (nếu có)
                if (!string.IsNullOrEmpty(bv.AnhTieuDe))
                    TryDeletePhysical(bv.AnhTieuDe);

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
