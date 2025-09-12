using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public class ScheduledPublishingService : IScheduledPublishingService
    {
        private readonly NewsDbContext _context;
        private readonly ILogger<ScheduledPublishingService> _logger;

        public ScheduledPublishingService(NewsDbContext context, ILogger<ScheduledPublishingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessScheduledPostsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Lấy các bài viết đã đến thời gian đăng
                var scheduledPosts = await _context.BaiViets
                    .Where(b => b.TrangThai == 2 && // Scheduled
                               b.NgayDangDuKien.HasValue &&
                               b.NgayDangDuKien.Value <= now)
                    .ToListAsync();

                if (!scheduledPosts.Any())
                {
                    _logger.LogInformation("Không có bài viết nào cần đăng lúc {Time}", now);
                    return;
                }

                _logger.LogInformation("Tìm thấy {Count} bài viết cần đăng lúc {Time}", scheduledPosts.Count, now);

                foreach (var post in scheduledPosts)
                {
                    // Cập nhật trạng thái thành Published
                    post.TrangThai = 1; // Published
                    post.NgayDang = now; // Cập nhật thời gian đăng thực tế
                    post.NgayDangDuKien = null; // Xóa thời gian lên lịch

                    _logger.LogInformation("Đã đăng bài viết: {Title} (ID: {Id})", post.TieuDe, post.Id);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã xử lý xong {Count} bài viết được lên lịch", scheduledPosts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý bài viết được lên lịch");
                throw;
            }
        }

        public async Task<bool> SchedulePostAsync(int baiVietId, DateTime scheduledTime)
        {
            try
            {
                var post = await _context.BaiViets.FindAsync(baiVietId);
                if (post == null)
                {
                    _logger.LogWarning("Không tìm thấy bài viết với ID: {Id}", baiVietId);
                    return false;
                }

                // Kiểm tra thời gian lên lịch phải trong tương lai
                if (scheduledTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Thời gian lên lịch phải trong tương lai. Bài viết ID: {Id}", baiVietId);
                    return false;
                }

                post.TrangThai = 2; // Scheduled
                post.NgayDangDuKien = scheduledTime;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã lên lịch đăng bài viết: {Title} (ID: {Id}) vào lúc {ScheduledTime}", 
                    post.TieuDe, post.Id, scheduledTime);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lên lịch đăng bài viết ID: {Id}", baiVietId);
                return false;
            }
        }

        public async Task<bool> UnschedulePostAsync(int baiVietId)
        {
            try
            {
                var post = await _context.BaiViets.FindAsync(baiVietId);
                if (post == null)
                {
                    _logger.LogWarning("Không tìm thấy bài viết với ID: {Id}", baiVietId);
                    return false;
                }

                post.TrangThai = 0; // Draft
                post.NgayDangDuKien = null;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã hủy lịch đăng bài viết: {Title} (ID: {Id})", post.TieuDe, post.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy lịch đăng bài viết ID: {Id}", baiVietId);
                return false;
            }
        }

        public async Task<List<BaiViet>> GetScheduledPostsAsync()
        {
            try
            {
                return await _context.BaiViets
                    .Where(b => b.TrangThai == 2) // Scheduled
                    .Include(b => b.ChuyenMuc)
                    .Include(b => b.TacGia)
                    .OrderBy(b => b.NgayDangDuKien)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết được lên lịch");
                return new List<BaiViet>();
            }
        }
    }
}
