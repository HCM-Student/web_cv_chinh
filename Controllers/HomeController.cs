using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.ViewModels; // nếu bạn đặt TinTucFilterVM trong Models/ViewModels

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

        // ===== Trang chủ: 3 bài mới nhất =====
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
            return View("LienHe", new LienHe());
        }

        // ===== Liên hệ (POST) =====
        [HttpPost, Route("Home/LienHe"), Route("lien-he")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LienHe([Bind("HoTen,Email,SoDienThoai,TieuDe,NoiDung")] LienHe model)
        {
            if (!ModelState.IsValid)
            {
                LogModelErrors(); // <-- method ở cuối file
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.";
                return View("LienHe", model);
            }

            try
            {
                model.NgayGui = DateTime.Now;
                _db.LienHes.Add(model);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn! Chúng tôi đã nhận được tin nhắn và sẽ phản hồi sớm nhất.";
                // Redirect để TempData hiển thị ở GET
                return RedirectToAction(nameof(LienHe));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lưu liên hệ");
                TempData["ErrorMessage"] = "Không thể gửi tin nhắn lúc này. Vui lòng thử lại sau.";
                return View("LienHe", model);
            }
        }

        // ===== Tin tức (lọc + sidebar) =====
        [HttpGet, Route("Home/TinTuc"), Route("tin-tuc")]
        public async Task<IActionResult> TinTuc(
            string? q, int? chuyenMucId, DateTime? from, DateTime? to,
            string sort = "newest", int page = 1, int pageSize = 9)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 9 : pageSize;

            var query = _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .Include(b => b.BinhLuans)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.TieuDe, $"%{kw}%") ||
                    (b.TomTat != null && EF.Functions.Like(b.TomTat, $"%{kw}%")) ||
                    EF.Functions.Like(b.NoiDung, $"%{kw}%"));
            }

            if (chuyenMucId.HasValue) query = query.Where(b => b.ChuyenMucId == chuyenMucId.Value);
            if (from.HasValue)        query = query.Where(b => b.NgayDang >= from.Value);
            if (to.HasValue)          query = query.Where(b => b.NgayDang <= to.Value);

            query = sort switch
            {
                "oldest"  => query.OrderBy(b => b.NgayDang),
                "az"      => query.OrderBy(b => b.TieuDe),
                "comment" => query.OrderByDescending(b => b.BinhLuans.Count),
                _         => query.OrderByDescending(b => b.NgayDang)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var cms = await _db.ChuyenMucs.AsNoTracking().OrderBy(c => c.Ten).ToListAsync();

            // Sidebar
            var latest5 = await _db.BaiViets
                .AsNoTracking()
                .OrderByDescending(x => x.NgayDang)
                .Take(5).ToListAsync();

            int? recruitCatId = await _db.ChuyenMucs
                .Where(c => EF.Functions.Like(c.Ten, "%tuyển%"))
                .Select(c => (int?)c.Id).FirstOrDefaultAsync();

            var recruit5 = await _db.BaiViets
                .AsNoTracking()
                .Where(b => recruitCatId != null && b.ChuyenMucId == recruitCatId.Value)
                .OrderByDescending(b => b.NgayDang)
                .Take(5).ToListAsync();

            var vm = new TinTucFilterVM
            {
                Q = q, ChuyenMucId = chuyenMucId, From = from, To = to,
                Sort = sort, Page = page, PageSize = pageSize,
                Items = items, Total = total, ChuyenMucs = cms,
                Latest5 = latest5, Recruit5 = recruit5
            };

            return View("~/Views/TinTuc/Index.cshtml", vm);
        }

        // ===== Chi tiết bài viết (hỗ trợ slug tùy chọn) =====
        [HttpGet]
        [Route("Home/ChiTietBaiViet/{id:int}")]
        [Route("bai-viet/{id:int}/{slug?}")]
        public async Task<IActionResult> ChiTietBaiViet(int id, string? slug = null)
        {
            var post = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .Include(b => b.BaiVietTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (post == null)
            {
                _logger.LogWarning("ChiTietBaiViet: Không tìm thấy bài viết Id={Id}", id);
                var hasAny = await _db.BaiViets.AsNoTracking().AnyAsync();
                if (!hasAny) return NotFound("Chưa có bài viết nào trong CSDL.");
                return NotFound($"Không tìm thấy bài viết (Id={id}).");
            }

            var expected = ToSlug(post.TieuDe ?? "");
            if (!string.IsNullOrWhiteSpace(slug) &&
                !string.Equals(slug, expected, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToActionPermanent(nameof(ChiTietBaiViet), new { id, slug = expected });
            }

            // Lấy bài viết liên quan (cùng chuyên mục, khác bài viết hiện tại)
            var relatedPosts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Where(b => b.ChuyenMucId == post.ChuyenMucId && b.Id != post.Id)
                .OrderByDescending(b => b.NgayDang)
                .Take(5)
                .ToListAsync();

            // Lấy bài viết mới nhất (khác bài viết hiện tại)
            var latestPosts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Where(b => b.Id != post.Id)
                .OrderByDescending(b => b.NgayDang)
                .Take(5)
                .ToListAsync();

            // Tạo ViewModel để truyền dữ liệu
            var viewModel = new ChiTietBaiVietVM
            {
                BaiViet = post,
                BaiVietLienQuan = relatedPosts,
                BaiVietMoiNhat = latestPosts
            };

            return View("ChiTietBaiViet", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        // ===== Helpers =====
        private void LogModelErrors()
        {
            var errs = ModelState
                .Where(kvp => kvp.Value?.Errors.Any() == true)
                .Select(kvp => $"{kvp.Key}: {string.Join(" | ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}");
            _logger.LogWarning("LienHe model invalid: {Errors}", string.Join(" || ", errs));
        }

        private static string ToSlug(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var norm = s.Normalize(System.Text.NormalizationForm.FormD);
            var filtered = new string(norm.Where(ch =>
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) !=
                System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
            filtered = filtered.Normalize(System.Text.NormalizationForm.FormC);
            filtered = new string(filtered.ToLowerInvariant().Select(c =>
                char.IsLetterOrDigit(c) ? c : '-').ToArray());
            while (filtered.Contains("--")) filtered = filtered.Replace("--", "-");
            return filtered.Trim('-');
        }

        [HttpPost]
        public async Task<IActionResult> IncrementViewCount([FromBody] int baiVietId)
        {
            try
            {
                var baiViet = await _db.BaiViets.FindAsync(baiVietId);
                if (baiViet != null)
                {
                    baiViet.LuotXem++;
                    await _db.SaveChangesAsync();
                    return Json(new { success = true, newCount = baiViet.LuotXem });
                }
                return Json(new { success = false, message = "Không tìm thấy bài viết" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật lượt xem: " + ex.Message });
            }
        }
    }
}
