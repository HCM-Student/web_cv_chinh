using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface IOnlineUserService
    {
        Task<int> GetOnlineUsersCountAsync(int minutes = 10);
        Task AddOrUpdateUserAsync(string sessionId, string? email = null, string? hoTen = null, string? ipAddress = null, string? userAgent = null);
        Task RemoveUserAsync(string sessionId);
        Task CleanupInactiveUsersAsync(int minutes = 10);
        Task<List<OnlineUser>> GetOnlineUsersAsync(int minutes = 10);
    }
}
