using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Models;
using WEB_CV.Data;
using System.Linq;

namespace WEB_CV.Services
{
    public class SiteCounter : ISiteCounter
    {
        private readonly NewsDbContext _db;
        private readonly ConcurrentDictionary<string, DateTime> _online = new();
        private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(10);

        private const string CookieName = "site.uid"; // nhận diện visitor trong 1 ngày
        private static readonly Regex BotRegex = new("(bot|crawl|spider|slurp|preview|scanner|facebookexternalhit|fetch)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SiteCounter(NewsDbContext db) => _db = db;

        public async Task TrackAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? "";

            // bỏ qua file tĩnh/thư mục thường gặp
            if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/img") ||
                path.StartsWith("/lib") || path.StartsWith("/fonts") || path.StartsWith("/static") ||
                path.StartsWith("/media") || path.StartsWith("/uploads") || path.StartsWith("/favicon.ico"))
                return;

            var ua = ctx.Request.Headers.UserAgent.ToString();
            var isBot = BotRegex.IsMatch(ua);

            // cookie nhận diện visitor (hết hạn 1 ngày => tính +1 mới)
            var isNewVisitor = false;
            if (!ctx.Request.Cookies.TryGetValue(CookieName, out var sid) || !Guid.TryParse(sid, out _))
            {
                sid = Guid.NewGuid().ToString();
                ctx.Response.Cookies.Append(CookieName, sid!, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(1),
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = false,
                    Secure = ctx.Request.IsHttps,
                    IsEssential = true
                });
                isNewVisitor = true;
            }

            // cập nhật "đang online"
            _online[sid!] = DateTime.UtcNow;

            if (!isBot && isNewVisitor)
                await IncrementTotalVisitsAsync();  // chỉ cộng khi là visitor mới trong ngày
        }

        public int GetOnline()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _online.ToArray())
                if (now - kv.Value > OnlineWindow)
                    _online.TryRemove(kv.Key, out _);
            return _online.Count;
        }

        public async Task<long> GetTotalVisitsAsync()
        {
            var stats = await _db.SiteStats.FirstOrDefaultAsync();
            return stats?.TotalVisits ?? 0;
        }

        private async Task IncrementTotalVisitsAsync()
        {
            // Kiểm tra xem có record nào chưa
            var existingStats = await _db.SiteStats.FirstOrDefaultAsync();
            
            if (existingStats == null)
            {
                // Tạo record đầu tiên
                _db.SiteStats.Add(new SiteStats { TotalVisits = 1 });
                await _db.SaveChangesAsync();
            }
            else
            {
                // Tăng lượt truy cập
                existingStats.TotalVisits++;
                existingStats.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
