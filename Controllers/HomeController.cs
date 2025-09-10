using System.Diagnostics;
using System.Linq; // cần cho .Where/.Select
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NewsDbContext _db;

        public HomeController(ILogger<HomeController> logger, NewsDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // ===== Trang chủ: trả 3 bài mới nhất =====
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var latest3 = await _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .OrderByDescending(x => x.NgayDang)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            return View(latest3);
        }

        // ===== Trang Giới thiệu =====
        [HttpGet, Route("Home/GioiThieu"), Route("gioi-thieu")]
        public IActionResult GioiThieu() => View("GioiThieu");

        public IActionResult Privacy() => View();

        // ===== Trang Thiết bị =====
        [HttpGet, Route("Home/ThietBi"), Route("thiet-bi")]
        public IActionResult ThietBi() => View("ThietBi");

        // ===== Trang Tuyển dụng =====
        [HttpGet, Route("Home/TuyenDung"), Route("tuyen-dung")]
        public IActionResult TuyenDung() => View("TuyenDung");

        // ===== Trang Báo giá =====
        [HttpGet, Route("Home/BaoGia"), Route("bao-gia")]
        public IActionResult BaoGia() => View("BaoGia");

        // ===== Liên hệ (GET) =====
        [HttpGet, Route("Home/LienHe"), Route("lien-he")]
        public IActionResult LienHe()
        {
            // Dùng Peek để KHÔNG tiêu thụ TempData, đồng thời copy sang ViewData
            if (TempData.ContainsKey("SuccessMessage"))
                ViewData["SuccessMessage"] = TempData.Peek("SuccessMessage") as string;

            if (TempData.ContainsKey("ErrorMessage"))
                ViewData["ErrorMessage"] = TempData.Peek("ErrorMessage") as string;

            return View("LienHe", new LienHe());
        }

        // ===== Liên hệ (POST) =====
        [HttpPost, Route("Home/LienHe"), Route("lien-he")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LienHe([Bind("HoTen,Email,SoDienThoai,TieuDe,NoiDung")] LienHe model)
        {
            if (!ModelState.IsValid)
            {
                LogModelErrors();
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.";
                return View("LienHe", model);
            }

            try
            {
                model.NgayGui = DateTime.Now;
                _db.LienHes.Add(model);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn! Chúng tôi đã nhận được tin nhắn và sẽ phản hồi sớm nhất.";
                // PRG: chuyển sang GET để hiển thị thông báo và tránh resubmit
                return RedirectToAction(nameof(LienHe));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lưu liên hệ");
                TempData["ErrorMessage"] = "Không thể gửi tin nhắn lúc này. Vui lòng thử lại sau.";
                return View("LienHe", model);
            }
        }

        // ===== Tin tức =====
        [HttpGet, Route("Home/TinTuc"), Route("tin-tuc")]
        public async Task<IActionResult> TinTuc()
        {
            var posts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            return View("~/Views/TinTuc/Index.cshtml", posts);
        }

        // ===== Chi tiết bài viết =====
        [HttpGet, Route("Home/ChiTietBaiViet/{id:int}"), Route("bai-viet/{id:int}")]
        public async Task<IActionResult> ChiTietBaiViet(int id)
        {
            var post = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (post == null) return NotFound();
            return View("~/Views/TinTuc/Details.cshtml", post);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        // ===== Helper: log lỗi model =====
        private void LogModelErrors()
        {
            var errs = ModelState
                .Where(kvp => kvp.Value?.Errors.Any() == true)
                .Select(kvp => $"{kvp.Key}: {string.Join(" | ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}");
            _logger.LogWarning("LienHe model invalid: {Errors}", string.Join(" || ", errs));
        }
    }
}
