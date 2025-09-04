using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // Lightweight JSON feed for notifications/recent activities
        [HttpGet]
        public async Task<IActionResult> RecentActivities(int take = 10)
        {
            var list = new List<ActivityVM>();

            var posts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.TacGia)
                .OrderByDescending(b => b.NgayDang)
                .Take(Math.Max(1, take))
                .ToListAsync();
            foreach (var p in posts)
            {
                var who = p.TacGia?.HoTen ?? "Admin";
                list.Add(new ActivityVM
                {
                    Type = "post",
                    Message = $"{who} đã tạo bài viết mới \"{p.TieuDe}\"",
                    CreatedAt = p.NgayDang
                });
            }

            var users = await _db.NguoiDungs
                .AsNoTracking()
                .OrderByDescending(u => u.NgayTao)
                .Take(Math.Max(1, take))
                .ToListAsync();
            foreach (var u in users)
            {
                list.Add(new ActivityVM
                {
                    Type = "user",
                    Message = $"{u.HoTen} đã đăng ký tài khoản mới",
                    CreatedAt = u.NgayTao
                });
            }

            var ordered = list
                .OrderByDescending(x => x.CreatedAt)
                .Take(Math.Max(1, take))
                .ToList();

            var bellCount = ordered.Count(x => (DateTime.UtcNow - x.CreatedAt.ToUniversalTime()).TotalHours < 24);
            var mailCount = 0; // chỗ trống để nối vào hệ thống tin nhắn sau

            return Json(new { bellCount, mailCount, activities = ordered });
        }

        public class ActivityVM
        {
            public string Type { get; set; } = string.Empty; // post | user | other
            public string Message { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
