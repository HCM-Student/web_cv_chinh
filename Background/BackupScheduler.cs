using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WEB_CV.Services.Backup;

namespace WEB_CV.Background
{
    /// <summary>
    /// Kiểm tra mỗi phút và chạy Full Backup đúng giờ theo cài đặt.
    /// Weekly = thứ Hai hàng tuần; Monthly = ngày 1 hằng tháng.
    /// </summary>
    public class BackupScheduler : BackgroundService
    {
        private readonly IBackupService _backup;
        private readonly ILogger<BackupScheduler> _logger;
        private DateTime? _lastRun;

        public BackupScheduler(IBackupService backup, ILogger<BackupScheduler> logger)
        {
            _backup = backup;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var settings = _backup.GetSettings();
                    if (settings.Enabled)
                    {
                        var now = DateTime.Now;
                        var parts = settings.TimeOfDay.Split(':');
                        var target = new DateTime(now.Year, now.Month, now.Day,
                                                  int.Parse(parts[0]), int.Parse(parts[1]), 0);

                        bool dueToday = now >= target && (_lastRun == null || _lastRun.Value.Date < now.Date);
                        bool freqOk = settings.Frequency switch
                        {
                            "Daily"   => true,
                            "Weekly"  => now.DayOfWeek == DayOfWeek.Monday,
                            "Monthly" => now.Day == 1,
                            _         => true
                        };

                        if (dueToday && freqOk)
                        {
                            _logger.LogInformation("Running scheduled full backup...");
                            await _backup.BackupFullAsync(stoppingToken);
                            _backup.EnforceRetention();
                            _lastRun = now;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BackupScheduler");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
