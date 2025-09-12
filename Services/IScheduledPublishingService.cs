using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface IScheduledPublishingService
    {
        /// <summary>
        /// Xử lý các bài viết đã đến thời gian đăng
        /// </summary>
        Task ProcessScheduledPostsAsync();

        /// <summary>
        /// Lên lịch đăng bài viết
        /// </summary>
        Task<bool> SchedulePostAsync(int baiVietId, DateTime scheduledTime);

        /// <summary>
        /// Hủy lịch đăng bài viết
        /// </summary>
        Task<bool> UnschedulePostAsync(int baiVietId);

        /// <summary>
        /// Lấy danh sách bài viết đã lên lịch
        /// </summary>
        Task<List<BaiViet>> GetScheduledPostsAsync();
    }
}
