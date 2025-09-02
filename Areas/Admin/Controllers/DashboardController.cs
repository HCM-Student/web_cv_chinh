using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly NewsDbContext _db;
        public DashboardController(NewsDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            // === DỮ LIỆU CHO VÒNG TRÒN: phân bố vai trò người dùng ===
            var roleCounts = await _db.NguoiDungs
                .AsNoTracking()
                .GroupBy(u => string.IsNullOrWhiteSpace(u.VaiTro) ? "Khác" : u.VaiTro)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            ViewBag.RoleLabels = roleCounts.Select(x => x.Role).ToArray();
            ViewBag.RoleData   = roleCounts.Select(x => x.Count).ToArray();

            // (tuỳ chọn) bạn có thể truyền thêm dữ liệu khác cho các chart/tile khác ở đây

            return View();
        }
    }
}
