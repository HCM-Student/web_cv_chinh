using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChuyenMucController : Controller
    {
        private readonly NewsDbContext _db;
        public ChuyenMucController(NewsDbContext db) { _db = db; }

        // GET: /Admin/ChuyenMuc
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.ChuyenMucs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.Ten.Contains(q));

            var list = await query.OrderBy(x => x.Id).ToListAsync();
            ViewBag.Q = q;
            return View(list);
        }

        // GET: /Admin/ChuyenMuc/Create
        public IActionResult Create() => View(new ChuyenMuc());

        // POST: /Admin/ChuyenMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ten,Slug,MoTa")] ChuyenMuc model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = Slugify(model.Ten);

            _db.ChuyenMucs.Add(model);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã thêm chuyên mục.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/ChuyenMuc/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cm = await _db.ChuyenMucs.FindAsync(id);
            if (cm == null) return NotFound();
            return View(cm);
        }

        // POST: /Admin/ChuyenMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ten,Slug,MoTa")] ChuyenMuc model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var cm = await _db.ChuyenMucs.FindAsync(id);
            if (cm == null) return NotFound();

            cm.Ten = model.Ten;
            cm.Slug = string.IsNullOrWhiteSpace(model.Slug) ? Slugify(model.Ten) : model.Slug;
            cm.MoTa = model.MoTa;

            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã cập nhật chuyên mục.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/ChuyenMuc/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cm = await _db.ChuyenMucs.FirstOrDefaultAsync(x => x.Id == id);
            if (cm == null) return NotFound();
            return View(cm);
        }

        // POST: /Admin/ChuyenMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cm = await _db.ChuyenMucs
                              .Include(x => x.BaiViets)
                              .FirstOrDefaultAsync(x => x.Id == id);
            if (cm == null) return NotFound();

            if (cm.BaiViets.Any())
            {
                TempData["msg"] = "Không thể xoá: còn bài viết thuộc chuyên mục này.";
                return RedirectToAction(nameof(Index));
            }

            _db.ChuyenMucs.Remove(cm);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã xoá chuyên mục.";
            return RedirectToAction(nameof(Index));
        }

        // --- helper ---
        private static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            string s = input.Trim().ToLowerInvariant();
            s = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in s)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) sb.Append(c);
                    else if (char.IsWhiteSpace(c) || c == '-' || c == '_') sb.Append('-');
                }
            }
            var slug = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
            return slug;
        }
    }
}
