using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;

        public DashboardController(NewsDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ===== ViewModel cho khối Media trên Dashboard =====
        public class MediaRecent
        {
            public string RelPath { get; set; } = "";
            public string Url { get; set; } = "";
            public long Bytes { get; set; }
            public DateTime Modified { get; set; }
        }

        public class MediaDashboardVm
        {
            public int TotalFiles { get; set; }
            public int TotalFolders { get; set; }
            public long TotalBytes { get; set; }
            public List<MediaRecent> Recent { get; set; } = new();
        }

        public static string HumanSize(long b)
        {
            string[] u = { "B", "KB", "MB", "GB", "TB" };
            double s = b; int i = 0;
            while (s >= 1024 && i < u.Length - 1) { s /= 1024; i++; }
            return $"{s:0.##} {u[i]}";
        }

        // ===== Dashboard =====
        public async Task<IActionResult> Index()
        {
            // === 1) DỮ LIỆU CHO VÒNG TRÒN: phân bố vai trò người dùng ===
            var roleCounts = await _db.NguoiDungs
                .AsNoTracking()
                .GroupBy(u => string.IsNullOrWhiteSpace(u.VaiTro) ? "Khác" : u.VaiTro)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            ViewBag.RoleLabels = roleCounts.Select(x => x.Role).ToArray();
            ViewBag.RoleData   = roleCounts.Select(x => x.Count).ToArray();

            // === 2) THỐNG KÊ MEDIA ===
            var mediaRoot = Path.Combine(_env.WebRootPath, "media");
            if (!Directory.Exists(mediaRoot)) Directory.CreateDirectory(mediaRoot);

            var allFiles = Directory.EnumerateFiles(mediaRoot, "*", SearchOption.AllDirectories).ToList();
            var allFolders = Directory.EnumerateDirectories(mediaRoot, "*", SearchOption.AllDirectories).Count();

            long totalBytes = 0;
            foreach (var f in allFiles) totalBytes += new FileInfo(f).Length;

            var imgExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".png",".jpg",".jpeg",".webp",".gif",".svg" };

            var recent = allFiles
                .Where(f => imgExts.Contains(Path.GetExtension(f)))
                .OrderByDescending(f => System.IO.File.GetLastWriteTimeUtc(f))
                .Take(8)
                .Select(f =>
                {
                    var rel = Path.GetRelativePath(mediaRoot, f).Replace("\\", "/");
                    return new MediaRecent
                    {
                        RelPath = rel,
                        Url = "/media/" + rel,
                        Bytes = new FileInfo(f).Length,
                        Modified = System.IO.File.GetLastWriteTimeUtc(f)
                    };
                })
                .ToList();

            var vm = new MediaDashboardVm
            {
                TotalFiles = allFiles.Count,
                TotalFolders = allFolders,
                TotalBytes = totalBytes,
                Recent = recent
            };

            // Trả view kèm model Media + ViewBag role cho chart
            return View(vm);
        }

        // ===== JSON feed cho hoạt động gần đây (chuông thông báo, …) =====
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

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Found {posts.Count} posts");

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

            // Nếu không có dữ liệu, tạo dữ liệu mẫu
            if (ordered.Count == 0)
            {
                ordered.Add(new ActivityVM
                {
                    Type = "post",
                    Message = "Hệ thống đã khởi động",
                    CreatedAt = DateTime.Now
                });
            }

            var bellCount = ordered.Count(x =>
                (DateTime.UtcNow - x.CreatedAt.ToUniversalTime()).TotalHours < 24);

            var mailCount = 0; // chỗ trống để nối hệ thống tin nhắn sau

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Returning {ordered.Count} activities");

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
