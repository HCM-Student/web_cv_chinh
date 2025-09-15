using WEB_CV.Services;

namespace WEB_CV.Services
{
    public class OnlineUserCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnlineUserCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Dọn dẹp mỗi 5 phút

        public OnlineUserCleanupService(IServiceProvider serviceProvider, ILogger<OnlineUserCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var onlineUserService = scope.ServiceProvider.GetRequiredService<IOnlineUserService>();
                        await onlineUserService.CleanupInactiveUsersAsync(10); // Dọn dẹp người dùng không hoạt động trong 10 phút
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during online user cleanup");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}

