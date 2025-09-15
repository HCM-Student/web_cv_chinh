using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichCongTacController : Controller
    {
        private readonly NewsDbContext _db;
        public LichCongTacController(NewsDbContext db) => _db = db;

        // GET: Admin/LichCongTac
        public async Task<IActionResult> Index()
        {
            var events = await _db.WorkScheduleEvents
                .OrderByDescending(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
            return View(events);
        }

        // GET: Admin/LichCongTac/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var workScheduleEvent = await _db.WorkScheduleEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workScheduleEvent == null) return NotFound();

            return View(workScheduleEvent);
        }

        // GET: Admin/LichCongTac/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/LichCongTac/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,StartTime,EndTime,Title,Location,Organization,Participants,Preparation,Contact,Phone,Email")] WorkScheduleEvent workScheduleEvent)
        {
            if (ModelState.IsValid)
            {
                _db.Add(workScheduleEvent);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm sự kiện lịch công tác thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(workScheduleEvent);
        }

        // GET: Admin/LichCongTac/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var workScheduleEvent = await _db.WorkScheduleEvents.FindAsync(id);
            if (workScheduleEvent == null) return NotFound();
            return View(workScheduleEvent);
        }

        // POST: Admin/LichCongTac/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,StartTime,EndTime,Title,Location,Organization,Participants,Preparation,Contact,Phone,Email")] WorkScheduleEvent workScheduleEvent)
        {
            if (id != workScheduleEvent.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(workScheduleEvent);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật sự kiện lịch công tác thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkScheduleEventExists(workScheduleEvent.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workScheduleEvent);
        }

        // GET: Admin/LichCongTac/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var workScheduleEvent = await _db.WorkScheduleEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workScheduleEvent == null) return NotFound();

            return View(workScheduleEvent);
        }

        // POST: Admin/LichCongTac/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workScheduleEvent = await _db.WorkScheduleEvents.FindAsync(id);
            if (workScheduleEvent != null)
            {
                _db.WorkScheduleEvents.Remove(workScheduleEvent);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sự kiện lịch công tác thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool WorkScheduleEventExists(int id)
        {
            return _db.WorkScheduleEvents.Any(e => e.Id == id);
        }
    }
}
