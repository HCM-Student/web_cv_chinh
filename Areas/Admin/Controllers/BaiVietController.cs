using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;               // IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Services;
using System.Web;
using Microsoft.AspNetCore.Http.Features;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BaiVietController : Controller
    {
        private readonly NewsDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ISEOAnalysisService _seoService;
        private readonly IScheduledPublishingService _scheduledPublishingService;

        public BaiVietController(NewsDbContext db, IWebHostEnvironment env, ISEOAnalysisService seoService, IScheduledPublishingService scheduledPublishingService)
        {
            _db = db;
            _env = env;
            _seoService = seoService;
            _scheduledPublishingService = scheduledPublishingService;
        }

        // ==== Cấu hình thư mục lưu ảnh tiêu đề ====
        private string ThumbRoot => Path.Combine(_env.WebRootPath, "media", "posts", "cover");
        private static readonly string[] allowedImg = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg" };

        // ==== Cấu hình thư mục lưu video ====
        private string VideoRoot => Path.Combine(_env.WebRootPath, "media", "posts", "videos");
        private static readonly string[] allowedVideo = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };

        private async Task<string> SaveThumbAsync(IFormFile f)
        {
            Directory.CreateDirectory(ThumbRoot);

            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!allowedImg.Contains(ext)) throw new Exception("Định dạng ảnh không hỗ trợ.");

            var baseName = Path.GetFileNameWithoutExtension(f.FileName).Trim();
            // làm sạch tên file
            var safe = string.Concat(baseName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch)).ToLower();
            if (string.IsNullOrWhiteSpace(safe)) safe = "thumb";

            var fileName = $"{safe}{ext}";
            var dst = Path.Combine(ThumbRoot, fileName);

            int i = 1;
            while (System.IO.File.Exists(dst))
            {
                fileName = $"{safe}-{i++}{ext}";
                dst = Path.Combine(ThumbRoot, fileName);
            }

            await using var s = System.IO.File.Create(dst);
            await f.CopyToAsync(s);

            // trả về URL public
            return $"/media/posts/cover/{fileName}";
        }

        private async Task<string> SaveVideoAsync(IFormFile f)
        {
            Directory.CreateDirectory(VideoRoot);

            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!allowedVideo.Contains(ext)) throw new Exception("Định dạng video không hỗ trợ.");

            var baseName = Path.GetFileNameWithoutExtension(f.FileName).Trim();
            // làm sạch tên file
            var safe = string.Concat(baseName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch)).ToLower();
            if (string.IsNullOrWhiteSpace(safe)) safe = "video";

            var fileName = $"{safe}{ext}";
            var dst = Path.Combine(VideoRoot, fileName);

            int i = 1;
            while (System.IO.File.Exists(dst))
            {
                fileName = $"{safe}-{i++}{ext}";
                dst = Path.Combine(VideoRoot, fileName);
            }

            await using var s = System.IO.File.Create(dst);
            await f.CopyToAsync(s);

            // trả về URL public
            return $"/media/posts/videos/{fileName}";
        }

        private void TryDeletePhysical(string? publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl)) return;
            var rel = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(_env.WebRootPath, rel);
            if (System.IO.File.Exists(full))
            {
                try { System.IO.File.Delete(full); } catch { /* ignore */ }
            }
        }

        private string ConvertToEmbedUrl(string videoUrl, string videoType)
        {
            if (string.IsNullOrEmpty(videoUrl)) return videoUrl;

            switch (videoType?.ToLower())
            {
                case "youtube":
                    // Chuyển đổi từ https://www.youtube.com/watch?v=... thành https://www.youtube.com/embed/...
                    if (videoUrl.Contains("youtube.com/watch?v="))
                    {
                        var videoId = ExtractYouTubeVideoId(videoUrl);
                        if (!string.IsNullOrEmpty(videoId))
                        {
                            return $"https://www.youtube.com/embed/{videoId}";
                        }
                    }
                    // Chuyển đổi từ https://youtu.be/... thành https://www.youtube.com/embed/...
                    else if (videoUrl.Contains("youtu.be/"))
                    {
                        var videoId = ExtractYouTubeShortVideoId(videoUrl);
                        if (!string.IsNullOrEmpty(videoId))
                        {
                            return $"https://www.youtube.com/embed/{videoId}";
                        }
                    }
                    // Nếu đã là embed URL thì giữ nguyên
                    else if (videoUrl.Contains("youtube.com/embed/"))
                    {
                        return videoUrl;
                    }
                    break;

                case "vimeo":
                    // Chuyển đổi từ https://vimeo.com/... thành https://player.vimeo.com/video/...
                    if (videoUrl.Contains("vimeo.com/"))
                    {
                        var videoId = ExtractVimeoVideoId(videoUrl);
                        if (!string.IsNullOrEmpty(videoId))
                        {
                            return $"https://player.vimeo.com/video/{videoId}";
                        }
                    }
                    // Nếu đã là player URL thì giữ nguyên
                    else if (videoUrl.Contains("player.vimeo.com/video/"))
                    {
                        return videoUrl;
                    }
                    break;
            }

            return videoUrl;
        }

        private string ExtractYouTubeVideoId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"] ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractYouTubeShortVideoId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var videoId = segments.LastOrDefault(s => !string.IsNullOrEmpty(s));
                
                // Loại bỏ query parameters nếu có
                if (!string.IsNullOrEmpty(videoId) && videoId.Contains('?'))
                {
                    videoId = videoId.Split('?')[0];
                }
                
                return videoId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractVimeoVideoId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                return segments.LastOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // GET: /Admin/BaiViet
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .OrderByDescending(x => x.NgayDang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x =>
                    x.TieuDe.Contains(q) ||
                    (x.TomTat ?? "").Contains(q) ||
                    (x.ChuyenMuc != null && x.ChuyenMuc.Ten.Contains(q)) ||
                    (x.TacGia != null && x.TacGia.HoTen.Contains(q))
                );
            }

            ViewBag.Q = q;
            return View(await query.ToListAsync());
        }

        // GET: /Admin/BaiViet/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new BaiViet());
        }

        // GET: /Admin/BaiViet/CreateEvent
        public async Task<IActionResult> CreateEvent()
        {
            await LoadDropdowns();
            var suKien = await _db.ChuyenMucs.FirstOrDefaultAsync(x => x.Ten == "Sự kiện");
            var model = new BaiViet
            {
                ChuyenMucId = suKien?.Id ?? 0,
                NgayDang = DateTime.UtcNow
            };
            ViewData["PresetCategoryName"] = suKien?.Ten ?? "";
            return View("Create", model);
        }

        // POST: /Admin/BaiViet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
        public async Task<IActionResult> Create(
            [Bind("TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang,AnhTieuDeAlt,TrangThai,NgayDangDuKien,VideoFile,VideoAlt,VideoUrl,VideoType")] BaiViet model,
            IFormFile? AnhTieuDeFile, IFormFile? VideoFile)
        {
            // bỏ validate navigation
            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            if (model.ChuyenMucId <= 0)
                ModelState.AddModelError(nameof(model.ChuyenMucId), "Vui lòng chọn chuyên mục.");
            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            // Xử lý scheduled publishing
            if (model.TrangThai == 2 && model.NgayDangDuKien.HasValue) // Scheduled
            {
                // Sử dụng timezone Việt Nam để so sánh
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                
                if (model.NgayDangDuKien.Value <= nowVietnam)
                {
                    ModelState.AddModelError(nameof(model.NgayDangDuKien), "Thời gian lên lịch phải trong tương lai.");
                }
                model.NgayDang = DateTime.UtcNow; // Thời gian tạo
            }
            else if (model.TrangThai == 1) // Published
            {
                if (model.NgayDang == default)
                    model.NgayDang = DateTime.UtcNow;
            }
            else // Draft
            {
                model.TrangThai = 0; // Draft
                model.NgayDang = DateTime.UtcNow; // Thời gian tạo
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            // Lưu ảnh tiêu đề (nếu có file)
            if (AnhTieuDeFile?.Length > 0)
            {
                try { model.AnhTieuDe = await SaveThumbAsync(AnhTieuDeFile); }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload ảnh lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
            }

            // Lưu video (nếu có file)
            if (VideoFile?.Length > 0)
            {
                try 
                { 
                    model.VideoFile = await SaveVideoAsync(VideoFile);
                    model.VideoType = "file";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload video lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
            }
            else if (!string.IsNullOrEmpty(model.VideoUrl) && !string.IsNullOrEmpty(model.VideoType))
            {
                // Chuyển đổi URL thành embed URL
                model.VideoUrl = ConvertToEmbedUrl(model.VideoUrl, model.VideoType);
            }

            try
            {
                _db.BaiViets.Add(model);
                await _db.SaveChangesAsync();
                
                string message = model.TrangThai switch
                {
                    0 => "Đã lưu bài viết dưới dạng bản nháp.",
                    1 => "Đã đăng bài viết thành công.",
                    2 => $"Đã lên lịch đăng bài viết vào lúc {model.NgayDangDuKien?.ToString("dd/MM/yyyy HH:mm")}.",
                    _ => "Đã thêm bài viết."
                };
                
                TempData["msg"] = message;
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, $"Lưu thất bại: {ex.InnerException?.Message ?? ex.Message}");
                await LoadDropdowns();
                return View(model);
            }
        }

        // GET: /Admin/BaiViet/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            await LoadDropdowns();
            return View(bv);
        }

        // POST: /Admin/BaiViet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TieuDe,TomTat,NoiDung,ChuyenMucId,TacGiaId,NgayDang,AnhTieuDeAlt,TrangThai,NgayDangDuKien,VideoFile,VideoAlt,VideoUrl,VideoType")] BaiViet model,
            IFormFile? AnhTieuDeFile, IFormFile? VideoFile, bool? removeThumb, bool? removeVideo)
        {
            if (id != model.Id) return BadRequest();

            ModelState.Remove("ChuyenMuc");
            ModelState.Remove("TacGia");

            if (model.ChuyenMucId <= 0)
                ModelState.AddModelError(nameof(model.ChuyenMucId), "Vui lòng chọn chuyên mục.");
            if (model.TacGiaId <= 0)
                ModelState.AddModelError(nameof(model.TacGiaId), "Vui lòng chọn tác giả.");

            // Xử lý scheduled publishing
            if (model.TrangThai == 2 && model.NgayDangDuKien.HasValue) // Scheduled
            {
                // Sử dụng timezone Việt Nam để so sánh
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                
                if (model.NgayDangDuKien.Value <= nowVietnam)
                {
                    ModelState.AddModelError(nameof(model.NgayDangDuKien), "Thời gian lên lịch phải trong tương lai.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            bv.TieuDe       = model.TieuDe;
            bv.TomTat       = model.TomTat;
            bv.NoiDung      = model.NoiDung;
            bv.ChuyenMucId  = model.ChuyenMucId;
            bv.TacGiaId     = model.TacGiaId;
            bv.NgayDang     = model.NgayDang;
            bv.AnhTieuDeAlt = model.AnhTieuDeAlt;
            bv.TrangThai    = model.TrangThai;
            bv.NgayDangDuKien = model.NgayDangDuKien;

            // xử lý ảnh: xoá / thay / giữ nguyên
            if (removeThumb == true && !string.IsNullOrEmpty(bv.AnhTieuDe))
            {
                TryDeletePhysical(bv.AnhTieuDe);
                bv.AnhTieuDe = null;
            }
            else if (AnhTieuDeFile?.Length > 0)
            {
                // thay ảnh mới -> xoá file cũ (nếu có), rồi lưu mới
                if (!string.IsNullOrEmpty(bv.AnhTieuDe))
                    TryDeletePhysical(bv.AnhTieuDe);

                try { bv.AnhTieuDe = await SaveThumbAsync(AnhTieuDeFile); }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload ảnh lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
            }

            // xử lý video: xoá / thay / giữ nguyên
            if (removeVideo == true && !string.IsNullOrEmpty(bv.VideoFile))
            {
                TryDeletePhysical(bv.VideoFile);
                bv.VideoFile = null;
                bv.VideoType = null;
            }
            else if (VideoFile?.Length > 0)
            {
                // thay video mới -> xoá file cũ (nếu có), rồi lưu mới
                if (!string.IsNullOrEmpty(bv.VideoFile))
                    TryDeletePhysical(bv.VideoFile);

                try 
                { 
                    bv.VideoFile = await SaveVideoAsync(VideoFile);
                    bv.VideoType = "file";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Upload video lỗi: {ex.Message}");
                    await LoadDropdowns();
                    return View(model);
                }
            }
            else
            {
                // Cập nhật thông tin video từ form (có thể là URL YouTube/Vimeo)
                bv.VideoFile = model.VideoFile;
                bv.VideoAlt = model.VideoAlt;
                bv.VideoType = model.VideoType;
                
                // Chuyển đổi URL thành embed URL nếu cần
                if (!string.IsNullOrEmpty(model.VideoUrl) && !string.IsNullOrEmpty(model.VideoType))
                {
                    bv.VideoUrl = ConvertToEmbedUrl(model.VideoUrl, model.VideoType);
                }
                else
                {
                    bv.VideoUrl = model.VideoUrl;
                }
            }

            try
            {
                await _db.SaveChangesAsync();
                TempData["msg"] = "Đã cập nhật bài viết.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, $"Cập nhật thất bại: {ex.InnerException?.Message ?? ex.Message}");
                await LoadDropdowns();
                return View(model);
            }
        }
            // GET: /Admin/BaiViet/Details/5
            [HttpGet]
            public async Task<IActionResult> Details(int id)
            {
                var bv = await _db.BaiViets
                    .Include(x => x.ChuyenMuc)
                    .Include(x => x.TacGia)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (bv == null) return NotFound();
                return View(bv);
            }

        // GET: /Admin/BaiViet/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var bv = await _db.BaiViets
                .Include(x => x.ChuyenMuc)
                .Include(x => x.TacGia)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bv == null) return NotFound();
            return View(bv);
        }

        // POST: /Admin/BaiViet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bv = await _db.BaiViets.FindAsync(id);
            if (bv == null) return NotFound();

            try
            {
                // xoá file ảnh kèm (nếu có)
                if (!string.IsNullOrEmpty(bv.AnhTieuDe))
                    TryDeletePhysical(bv.AnhTieuDe);

                _db.BaiViets.Remove(bv);
                await _db.SaveChangesAsync();
                TempData["msg"] = "Đã xoá bài viết.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["msg"] = $"Không thể xoá: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/BaiViet/SEOAnalysis/5
        public async Task<IActionResult> SEOAnalysis(int id)
        {
            var baiViet = await _db.BaiViets.FindAsync(id);
            if (baiViet == null) return NotFound();

            try
            {
                var analysis = await _seoService.AnalyzeBaiVietAsync(id);
                return View(analysis);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi phân tích SEO: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/BaiViet/Scheduled
        public async Task<IActionResult> Scheduled()
        {
            var scheduledPosts = await _scheduledPublishingService.GetScheduledPostsAsync();
            return View(scheduledPosts);
        }

        // POST: /Admin/BaiViet/Schedule/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(int id, DateTime scheduledTime)
        {
            var success = await _scheduledPublishingService.SchedulePostAsync(id, scheduledTime);
            if (success)
            {
                TempData["msg"] = "Đã lên lịch đăng bài viết thành công.";
            }
            else
            {
                TempData["msg"] = "Không thể lên lịch đăng bài viết.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/BaiViet/Unschedule/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unschedule(int id)
        {
            var success = await _scheduledPublishingService.UnschedulePostAsync(id);
            if (success)
            {
                TempData["msg"] = "Đã hủy lịch đăng bài viết.";
            }
            else
            {
                TempData["msg"] = "Không thể hủy lịch đăng bài viết.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            ViewBag.ChuyenMucId = new SelectList(
                await _db.ChuyenMucs.OrderBy(x => x.Ten).ToListAsync(), "Id", "Ten");

            ViewBag.TacGiaId = new SelectList(
                await _db.NguoiDungs.OrderBy(x => x.HoTen).ToListAsync(), "Id", "HoTen");
        }
    }
}
