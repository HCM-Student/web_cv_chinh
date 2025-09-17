using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.ViewModels;

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
                .Where(x => x.TrangThai == 1) // Chỉ lấy bài viết đã đăng
                .OrderByDescending(x => x.NgayDang)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            return View(latest3);
        }

        // ===== Trang Giới thiệu =====
        [HttpGet, Route("Home/GioiThieu"), Route("gioi-thieu")]
        public IActionResult GioiThieu() => View("GioiThieu");

        // ===== Trang Giới thiệu chung =====
        [HttpGet, Route("Home/GioiThieuChung"), Route("gioi-thieu-chung")]
        public IActionResult GioiThieuChung() => View("GioiThieuChung");

        // ===== Trang Lãnh đạo bộ =====
        [HttpGet, Route("Home/LanhDaoBo"), Route("lanh-dao-bo")]
        public IActionResult LanhDaoBo() => View("LanhDaoBo");

        // ===== Trang Chức năng nhiệm vụ =====
        [HttpGet, Route("Home/ChucNangNhiemVu"), Route("chuc-nang-nhiem-vu")]
        public IActionResult ChucNangNhiemVu() => View("ChucNangNhiemVu");

        // ===== Trang Cơ cấu tổ chức =====
        [HttpGet, Route("Home/CoCauToChuc"), Route("co-cau-to-chuc")]
        public IActionResult CoCauToChuc() => View("CoCauToChuc");

        // ===== Trang Quá trình phát triển =====
        [HttpGet, Route("Home/QuaTrinhPhatTrien"), Route("qua-trinh-phat-trien")]
        public IActionResult QuaTrinhPhatTrien() => View("QuaTrinhPhatTrien");

        // ===== Trang Danh bạ các đơn vị =====
        [HttpGet, Route("Home/DanhBaDonVi"), Route("danh-ba-don-vi")]
        public IActionResult DanhBaDonVi() => View("DanhBaDonVi");

        public IActionResult Privacy() => View();

        // ===== Trang Tuyển dụng =====
        [HttpGet, Route("Home/TuyenDung"), Route("tuyen-dung")]
        public IActionResult TuyenDung() => View("TuyenDung");

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
            _logger.LogInformation("LienHe POST: Model received - HoTen: {HoTen}, Email: {Email}, TieuDe: {TieuDe}", 
                model.HoTen, model.Email, model.TieuDe);

            if (!ModelState.IsValid)
            {
                LogModelErrors(); // <-- method ở cuối file
                _logger.LogWarning("Model validation failed for LienHe");
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.";
                return View("LienHe", model);
            }

            try
            {
                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chưa đọc";
                model.DaXuLy = false;
                
                _logger.LogInformation("Saving LienHe to database: {Model}", model);
                _db.LienHes.Add(model);
                await _db.SaveChangesAsync();

                _logger.LogInformation("LienHe saved successfully with ID: {Id}", model.Id);
                TempData["SuccessMessage"] = "Cảm ơn bạn! Chúng tôi đã nhận được tin nhắn và sẽ phản hồi sớm nhất.";
                // Redirect để TempData hiển thị ở GET
                return RedirectToAction(nameof(LienHe));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lưu liên hệ: {Message}", ex.Message);
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
                .Where(b => b.TrangThai == 1) // Chỉ lấy bài viết đã đăng
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
                .Where(x => x.TrangThai == 1) // Chỉ lấy bài viết đã đăng
                .OrderByDescending(x => x.NgayDang)
                .Take(5).ToListAsync();

            int? recruitCatId = await _db.ChuyenMucs
                .Where(c => EF.Functions.Like(c.Ten, "%tuyển%"))
                .Select(c => (int?)c.Id).FirstOrDefaultAsync();

            var recruit5 = await _db.BaiViets
                .AsNoTracking()
                .Where(b => recruitCatId != null && b.ChuyenMucId == recruitCatId.Value && b.TrangThai == 1)
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
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .Include(b => b.BaiVietTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id && b.TrangThai == 1); // Chỉ lấy bài viết đã đăng

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

            // Tăng lượt xem
            post.LuotXem++;
            await _db.SaveChangesAsync();

            // Lấy bài viết liên quan (cùng chuyên mục, khác bài viết hiện tại)
            var relatedPosts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Where(b => b.ChuyenMucId == post.ChuyenMucId && b.Id != post.Id && b.TrangThai == 1)
                .OrderByDescending(b => b.NgayDang)
                .Take(5)
                .ToListAsync();

            // Lấy bài viết mới nhất (khác bài viết hiện tại)
            var latestPosts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Where(b => b.Id != post.Id && b.TrangThai == 1)
                .OrderByDescending(b => b.NgayDang)
                .Take(12)
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

        // ===== Trang dấu trang (chỉ dành cho người dùng thường) =====
        [HttpGet]
        public IActionResult Bookmarks()
        {
            // Kiểm tra xem user có phải admin không
            if (User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Admin không có chức năng dấu trang. Vui lòng sử dụng quản trị để quản lý bài viết.";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Kiểm tra đăng nhập
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Dấu trang của tôi";
            return View();
        }

        // ===== DANH MỤC PAGES =====
        [HttpGet, Route("Home/DauTuDauThau"), Route("dau-tu-dau-thau")]
        public IActionResult DauTuDauThau()
        {
            ViewData["Title"] = "Đầu tư, đấu thầu";
            return View();
        }

        [HttpGet, Route("Home/DuAnDauTu"), Route("du-an-dau-tu")]
        public IActionResult DuAnDauTu()
        {
            ViewData["Title"] = "Dự án đầu tư";
            return View();
        }

        [HttpGet, Route("Home/ThongTinDauThau"), Route("thong-tin-dau-thau")]
        public IActionResult ThongTinDauThau()
        {
            ViewData["Title"] = "Thông tin đấu thầu";
            return View();
        }

        [HttpGet, Route("Home/SanPham"), Route("san-pham")]
        public IActionResult SanPham()
        {
            ViewData["Title"] = "Sản phẩm";
            return View();
        }

    [HttpGet, Route("Home/ThongBaoNoiBo"), Route("thong-bao-noi-bo")]
    public async Task<IActionResult> ThongBaoNoiBo(string keyword = "", string fromDate = "", string toDate = "", int pageSize = 10, int page = 1, bool? isPinned = null)
    {
        ViewData["Title"] = "Thông báo nội bộ";

        // Kiểm tra xem có thông báo nào không
        var count = await _db.InternalNotices.CountAsync();
        Console.WriteLine($"Số lượng thông báo trong database: {count}");
        
        // Nếu chưa có thông báo nào, tạo một thông báo mẫu
        if (count == 0)
        {
            Console.WriteLine("Tạo thông báo mẫu...");
            var thongBaoMau = new InternalNotice
            {
                Title = "Thông báo mẫu - Chào mừng đến với hệ thống",
                Summary = "Đây là thông báo mẫu để test hệ thống thông báo nội bộ.",
                Body = "<p>Xin chào các đồng chí!</p><p>Đây là thông báo mẫu để kiểm tra hệ thống thông báo nội bộ của Cục Chuyển đổi số.</p><p>Hệ thống đã hoạt động bình thường và sẵn sàng phục vụ.</p>",
                PublishAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                IsPinned = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByName = "Hệ thống"
            };
            
            _db.InternalNotices.Add(thongBaoMau);
            await _db.SaveChangesAsync();
            Console.WriteLine("Đã tạo thông báo mẫu thành công!");
        }

        // Tạo query cơ bản
        var query = _db.InternalNotices.AsQueryable();

        // Lọc theo trạng thái hoạt động
        query = query.Where(tb => tb.IsActive);

        // Tìm kiếm theo từ khóa
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var searchTerm = keyword.ToLower();
            query = query.Where(tb => 
                tb.Title.ToLower().Contains(searchTerm) ||
                (tb.Summary != null && tb.Summary.ToLower().Contains(searchTerm)) ||
                (tb.Body != null && tb.Body.ToLower().Contains(searchTerm))
            );
        }

        // Lọc theo ngày bắt đầu
        if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out var from))
        {
            query = query.Where(tb => tb.PublishAt.Date >= from.Date);
        }

        // Lọc theo ngày kết thúc
        if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out var to))
        {
            query = query.Where(tb => tb.PublishAt.Date <= to.Date);
        }

        // Lọc theo trạng thái ghim
        if (isPinned.HasValue)
        {
            query = query.Where(tb => tb.IsPinned == isPinned.Value);
        }

        // Đếm tổng số kết quả
        var totalCount = await query.CountAsync();

        // Sắp xếp và phân trang
        var notices = await query
            .OrderByDescending(tb => tb.IsPinned)
            .ThenByDescending(tb => tb.PublishAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Tạo ViewModel
        var searchVm = new ThongBaoNoiBoSearchVm
        {
            Keyword = keyword,
            FromDate = !string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out var fromDateParsed) ? fromDateParsed : null,
            ToDate = !string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out var toDateParsed) ? toDateParsed : null,
            PageSize = pageSize,
            Page = page,
            IsPinned = isPinned,
            Notices = notices,
            TotalCount = totalCount
        };

        Console.WriteLine($"Số lượng thông báo sau filter: {notices.Count}");

        return View(searchVm);
    }

    [HttpGet, Route("Home/ChiTietThongBaoNoiBo/{id}"), Route("thong-bao-noi-bo/{id}")]
    public async Task<IActionResult> ChiTietThongBaoNoiBo(int id)
    {
        Console.WriteLine($"Trying to get notice with ID: {id}");
        
        var thongBao = await _db.InternalNotices
            .FirstOrDefaultAsync(tb => tb.Id == id && tb.IsActive);

        if (thongBao == null)
        {
            Console.WriteLine($"Notice with ID {id} not found or not active");
            return NotFound();
        }

        Console.WriteLine($"Found notice: {thongBao.Title}");
        ViewData["Title"] = thongBao.Title;
        return View(thongBao);
    }

    }
}
