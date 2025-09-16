using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.ViewModels;

namespace WEB_CV.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class LichCongTacController : Controller
    {
        private readonly NewsDbContext _db;
        public LichCongTacController(NewsDbContext db) => _db = db;

        // /lich-cong-tac?start=2025-09-15&leader=...&q=...
        [HttpGet("/lich-cong-tac")]
        public async Task<IActionResult> Index(DateTime? start, string? leader, string? q)
        {
            var monday = GetMonday(start ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var baseQuery = _db.WorkScheduleEvents
                .Where(e => e.Scope == ScheduleScope.DonVi && e.Date >= monday && e.Date <= sunday);

            if (!string.IsNullOrWhiteSpace(leader))
                baseQuery = baseQuery.Where(e => e.Leader == leader);

            if (!string.IsNullOrWhiteSpace(q))
                baseQuery = baseQuery.Where(e =>
                    EF.Functions.Like(e.Title, $"%{q}%") ||
                    EF.Functions.Like(e.Organization, $"%{q}%") ||
                    EF.Functions.Like(e.Location, $"%{q}%"));

            var list = await baseQuery
                .AsNoTracking()
                .OrderBy(e => e.Date).ThenBy(e => e.StartTime)
                .ToListAsync();

            var vm = new WorkScheduleVm
            {
                WeekStart = monday,
                WeekEnd = sunday,
                WeekNo = ISOWeek.GetWeekOfYear(monday),
                Leader = leader,
                Keyword = q,
                LeaderOptions = await _db.WorkScheduleEvents
                                         .Where(e => e.Scope == ScheduleScope.DonVi)
                                         .Select(e => e.Leader)
                                         .Distinct().OrderBy(x => x).ToListAsync()
            };

            for (int i = 0; i < 7; i++)
            {
                var d = monday.AddDays(i).Date;
                vm.EventsByDay[d] = list.Where(x => x.Date.Date == d).ToList();
            }

            return View(vm);
        }

        private static DateTime GetMonday(DateTime d)
        {
            int diff = (7 + (int)d.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return d.AddDays(-diff).Date;
        }
    }
}
