using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_CV.Data;
using Microsoft.EntityFrameworkCore;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class BaoCaoController : Controller
    {
        private readonly NewsDbContext _context;

        public BaoCaoController(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            var totalBaiViet = await _context.BaiViets.CountAsync();
            var totalChuyenMuc = await _context.ChuyenMucs.CountAsync();
            var totalNguoiDung = await _context.NguoiDungs.CountAsync();
            var totalLienHe = await _context.LienHes.CountAsync();
            var totalLuotXem = await _context.BaiViets.SumAsync(b => b.LuotXem);

            // Thống kê theo tháng (6 tháng gần nhất)
            var startDate = DateTime.Now.AddMonths(-6);
            var monthlyStats = await _context.BaiViets
                .Where(b => b.NgayDang >= startDate)
                .GroupBy(b => new { b.NgayDang.Year, b.NgayDang.Month })
                .Select(g => new
                {
                    Thang = g.Key.Month,
                    Nam = g.Key.Year,
                    SoBaiViet = g.Count(),
                    TongLuotXem = g.Sum(b => b.LuotXem)
                })
                .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
                .ToListAsync();

            // Top 10 bài viết được xem nhiều nhất
            var topBaiViet = await _context.BaiViets
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .OrderByDescending(b => b.LuotXem)
                .Take(10)
                .Select(b => new
                {
                    b.Id,
                    b.TieuDe,
                    b.LuotXem,
                    b.NgayDang,
                    ChuyenMuc = b.ChuyenMuc!.Ten,
                    TacGia = b.TacGia!.HoTen
                })
                .ToListAsync();

            // Thống kê theo chuyên mục
            var statsByChuyenMuc = await _context.ChuyenMucs
                .Select(c => new
                {
                    c.Ten,
                    SoBaiViet = _context.BaiViets.Count(b => b.ChuyenMucId == c.Id),
                    TongLuotXem = _context.BaiViets.Where(b => b.ChuyenMucId == c.Id).Sum(b => b.LuotXem)
                })
                .OrderByDescending(x => x.SoBaiViet)
                .ToListAsync();

            // Thống kê liên hệ
            var lienHeChuaXuLy = await _context.LienHes.CountAsync(l => !l.DaXuLy);
            var lienHeDaXuLy = await _context.LienHes.CountAsync(l => l.DaXuLy);

            // Thống kê người dùng theo vai trò
            var userStats = await _context.NguoiDungs
                .GroupBy(u => u.VaiTro)
                .Select(g => new
                {
                    VaiTro = string.IsNullOrEmpty(g.Key) ? "Khác" : g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            ViewBag.TotalBaiViet = totalBaiViet;
            ViewBag.TotalChuyenMuc = totalChuyenMuc;
            ViewBag.TotalNguoiDung = totalNguoiDung;
            ViewBag.TotalLienHe = totalLienHe;
            ViewBag.TotalLuotXem = totalLuotXem;
            ViewBag.MonthlyStats = monthlyStats;
            ViewBag.TopBaiViet = topBaiViet;
            ViewBag.StatsByChuyenMuc = statsByChuyenMuc;
            ViewBag.LienHeChuaXuLy = lienHeChuaXuLy;
            ViewBag.LienHeDaXuLy = lienHeDaXuLy;
            ViewBag.UserStats = userStats;

            return View();
        }
    }
}
