using WEB_CV.Services;

namespace WEB_CV.Services
{
    public class ScheduledPublishingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledPublishingBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // Kiểm tra mỗi phút

        public ScheduledPublishingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledPublishingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled Publishing Background Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scheduledPublishingService = scope.ServiceProvider.GetRequiredService<IScheduledPublishingService>();
                        await scheduledPublishingService.ProcessScheduledPostsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Scheduled Publishing Background Service");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Scheduled Publishing Background Service đã dừng");
        }
    }
}
