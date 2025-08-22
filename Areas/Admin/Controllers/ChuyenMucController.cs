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
            {
                query = query.Where(x => x.Ten.Contains(q));
            }

            var list = await query.OrderBy(x => x.Ten).ToListAsync();
            ViewBag.Q = q;
            return View(list);
        }

        // GET: /Admin/ChuyenMuc/Create
        public IActionResult Create() => View(new ChuyenMuc());

        // POST: /Admin/ChuyenMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ten")] ChuyenMuc model)
        {
            if (!ModelState.IsValid) return View(model);

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ten")] ChuyenMuc model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var cm = await _db.ChuyenMucs.FindAsync(id);
            if (cm == null) return NotFound();

            cm.Ten = model.Ten;
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
            var cm = await _db.ChuyenMucs.FindAsync(id);
            if (cm == null) return NotFound();

            // Lưu ý: nếu bảng BàiViết đang dùng FK tới chuyên mục, cần xử lý ràng buộc trước khi xoá
            _db.ChuyenMucs.Remove(cm);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Đã xoá chuyên mục.";
            return RedirectToAction(nameof(Index));
        }
    }
}
