using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.ViewModels;

namespace WEB_CV.Controllers
{
    public class LichCongTacController : Controller
    {
        private readonly NewsDbContext _db;
        public LichCongTacController(NewsDbContext db) => _db = db;

        // /lich-cong-tac?start=2025-09-15
        [HttpGet("/lich-cong-tac")]
        public async Task<IActionResult> Index(DateTime? start, string? keyword, string? org, string? email, string? phone, string? contact)
        {
            var today = DateTime.Today;
            var startOfWeek = GetMonday(start ?? today);
            var endOfWeek = startOfWeek.AddDays(6);

            var q = _db.WorkScheduleEvents
                .Where(e => e.Date >= startOfWeek && e.Date <= endOfWeek);

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(e => EF.Functions.Like(e.Title, $"%{keyword}%"));
            if (!string.IsNullOrWhiteSpace(org))
                q = q.Where(e => EF.Functions.Like(e.Organization, $"%{org}%"));
            if (!string.IsNullOrWhiteSpace(email))
                q = q.Where(e => e.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(phone))
                q = q.Where(e => e.Phone.Contains(phone));
            if (!string.IsNullOrWhiteSpace(contact))
                q = q.Where(e => EF.Functions.Like(e.Contact, $"%{contact}%"));

            var list = await q.AsNoTracking()
                              .OrderBy(e => e.Date)
                              .ThenBy(e => e.StartTime)
                              .ToListAsync();

            var vm = new WorkScheduleVm {
                WeekStart = startOfWeek,
                WeekEnd = endOfWeek,
                Keyword = keyword, Org = org, Email = email, Phone = phone, Contact = contact
            };

            // tạo đủ 7 ngày
            for (int i = 0; i < 7; i++)
            {
                var d = startOfWeek.AddDays(i).Date;
                vm.EventsByDay[d] = list.Where(x => x.Date.Date == d).ToList();
            }

            return View(vm);
        }

        private static DateTime GetMonday(DateTime d)
        {
            int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            return d.AddDays(-1 * diff).Date;
        }
    }
}
