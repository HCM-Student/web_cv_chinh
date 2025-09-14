using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public class OnlineUserService : IOnlineUserService
    {
        private readonly NewsDbContext _db;
        private readonly ILogger<OnlineUserService> _logger;

        public OnlineUserService(NewsDbContext db, ILogger<OnlineUserService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> GetOnlineUsersCountAsync(int minutes = 10)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
                return await _db.OnlineUsers
                    .Where(u => u.IsActive && u.LastSeen >= cutoffTime)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users count");
                return 0;
            }
        }

        public async Task AddOrUpdateUserAsync(string sessionId, string? email = null, string? hoTen = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var existingUser = await _db.OnlineUsers
                    .FirstOrDefaultAsync(u => u.SessionId == sessionId);

                var currentTime = DateTime.UtcNow;

                if (existingUser != null)
                {
                    // Chỉ cập nhật LastSeen nếu đã hơn 5 phút từ lần cuối
                    var timeDiff = currentTime.Subtract(existingUser.LastSeen).TotalMinutes;
                    
                    if (timeDiff > 5)
                    {
                        existingUser.LastSeen = currentTime;
                        existingUser.IsActive = true;
                        
                        // Chỉ cập nhật thông tin nếu có thay đổi
                        if (!string.IsNullOrEmpty(email) && existingUser.Email != email) existingUser.Email = email;
                        if (!string.IsNullOrEmpty(hoTen) && existingUser.HoTen != hoTen) existingUser.HoTen = hoTen;
                        if (!string.IsNullOrEmpty(ipAddress) && existingUser.IpAddress != ipAddress) existingUser.IpAddress = ipAddress;
                        if (!string.IsNullOrEmpty(userAgent) && existingUser.UserAgent != userAgent) existingUser.UserAgent = userAgent;
                        
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    // Thêm người dùng mới
                    var newUser = new OnlineUser
                    {
                        SessionId = sessionId,
                        Email = email,
                        HoTen = hoTen,
                        LastSeen = currentTime,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        IsActive = true
                    };
                    _db.OnlineUsers.Add(newUser);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating online user");
            }
        }

        public async Task RemoveUserAsync(string sessionId)
        {
            try
            {
                var user = await _db.OnlineUsers
                    .FirstOrDefaultAsync(u => u.SessionId == sessionId);

                if (user != null)
                {
                    user.IsActive = false;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing online user");
            }
        }

        public async Task CleanupInactiveUsersAsync(int minutes = 10)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
                var inactiveUsers = await _db.OnlineUsers
                    .Where(u => u.LastSeen < cutoffTime)
                    .ToListAsync();

                foreach (var user in inactiveUsers)
                {
                    user.IsActive = false;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive users");
            }
        }

        public async Task<List<OnlineUser>> GetOnlineUsersAsync(int minutes = 10)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
                return await _db.OnlineUsers
                    .Where(u => u.IsActive && u.LastSeen >= cutoffTime)
                    .OrderByDescending(u => u.LastSeen)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return new List<OnlineUser>();
            }
        }
    }
}
