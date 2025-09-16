using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class LichCongTacController : Controller
    {
        private readonly NewsDbContext _db;
        public LichCongTacController(NewsDbContext db) => _db = db;

        // GET: Admin/LichCongTac
        public async Task<IActionResult> Index()
        {
            var list = await _db.WorkScheduleEvents
                .Where(x=>x.Scope==ScheduleScope.DonVi)
                .OrderByDescending(x=>x.Date).ThenBy(x=>x.StartTime)
                .Take(200).ToListAsync();
            return View(list);
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
            return View(new WorkScheduleEvent{ Date=DateTime.Today, Scope=ScheduleScope.DonVi });
        }

        // POST: Admin/LichCongTac/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,StartTime,EndTime,Title,Leader,Location,Organization,Participants,Preparation,Contact,Phone,Email,Scope")] WorkScheduleEvent workScheduleEvent)
        {
            workScheduleEvent.Scope = ScheduleScope.DonVi; // luôn là đơn vị
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,StartTime,EndTime,Title,Leader,Location,Organization,Participants,Preparation,Contact,Phone,Email,Scope")] WorkScheduleEvent workScheduleEvent)
        {
            if (id != workScheduleEvent.Id) return NotFound();

            workScheduleEvent.Scope = ScheduleScope.DonVi; // giữ đúng phạm vi
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

        // POST: Admin/LichCongTac/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

        // GET: Admin/LichCongTac/CreateTestData
        public async Task<IActionResult> CreateTestData()
        {
            var testData = new List<WorkScheduleEvent>
            {
                new WorkScheduleEvent
                {
                    Date = DateTime.Today.AddDays(1),
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(10, 0, 0),
                    Title = "Họp giao ban tuần",
                    Leader = "Nguyễn Văn A",
                    Location = "Phòng họp A1",
                    Organization = "Cục Chuyển đổi số",
                    Participants = "Lãnh đạo các phòng ban",
                    Preparation = "Chuẩn bị báo cáo tuần",
                    Contact = "Nguyễn Thị B",
                    Phone = "0123456789",
                    Email = "nguyenvana@example.com",
                    Scope = ScheduleScope.DonVi
                },
                new WorkScheduleEvent
                {
                    Date = DateTime.Today.AddDays(2),
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Title = "Kiểm tra dự án số hóa",
                    Leader = "Trần Văn C",
                    Location = "Phòng IT",
                    Organization = "Cục Chuyển đổi số",
                    Participants = "Đội dự án, Kỹ thuật viên",
                    Preparation = "Chuẩn bị demo sản phẩm",
                    Contact = "Lê Văn D",
                    Phone = "0987654321",
                    Email = "tranvanc@example.com",
                    Scope = ScheduleScope.DonVi
                },
                new WorkScheduleEvent
                {
                    Date = DateTime.Today.AddDays(3),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(11, 30, 0),
                    Title = "Tập huấn công nghệ mới",
                    Leader = "Phạm Thị E",
                    Location = "Hội trường lớn",
                    Organization = "Cục Chuyển đổi số",
                    Participants = "Toàn thể cán bộ",
                    Preparation = "Chuẩn bị tài liệu, máy chiếu",
                    Contact = "Hoàng Văn F",
                    Phone = "0369258147",
                    Email = "phamthie@example.com",
                    Scope = ScheduleScope.DonVi
                }
            };

            foreach (var item in testData)
            {
                _db.WorkScheduleEvents.Add(item);
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo dữ liệu test thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool WorkScheduleEventExists(int id)
        {
            return _db.WorkScheduleEvents.Any(e => e.Id == id);
        }
    }
}
