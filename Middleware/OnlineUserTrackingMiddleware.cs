using WEB_CV.Services;
using Microsoft.Extensions.DependencyInjection;

namespace WEB_CV.Middleware
{
    public class OnlineUserTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OnlineUserTrackingMiddleware> _logger;

        public OnlineUserTrackingMiddleware(RequestDelegate next, ILogger<OnlineUserTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            try
            {
                // Chỉ theo dõi các request GET cho trang chính (tránh theo dõi API calls, POST requests, static files)
                if (context.Request.Method == "GET" && 
                    !context.Request.Path.StartsWithSegments("/api") &&
                    !context.Request.Path.StartsWithSegments("/css") &&
                    !context.Request.Path.StartsWithSegments("/js") &&
                    !context.Request.Path.StartsWithSegments("/lib") &&
                    !context.Request.Path.StartsWithSegments("/media") &&
                    !context.Request.Path.StartsWithSegments("/favicon.ico") &&
                    context.Request.Path != "/")
                {
                    var sessionId = context.Session.Id;
                    var userEmail = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    var userHoTen = context.User?.Identity?.Name;
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                    var userAgent = context.Request.Headers["User-Agent"].ToString();

                    // Chỉ track khi session chưa được track trong 5 phút gần đây
                    var lastTrackTime = context.Session.GetString("LastTrackTime");
                    var shouldTrack = string.IsNullOrEmpty(lastTrackTime) || 
                        !DateTime.TryParse(lastTrackTime, out var lastTrack) ||
                        DateTime.UtcNow.Subtract(lastTrack).TotalMinutes > 5;

                    if (shouldTrack)
                    {
                        // Theo dõi người dùng với scope riêng
                        try
                        {
                            using (var scope = serviceProvider.CreateScope())
                            {
                                var onlineUserService = scope.ServiceProvider.GetRequiredService<IOnlineUserService>();
                                await onlineUserService.AddOrUpdateUserAsync(sessionId, userEmail, userHoTen, ipAddress, userAgent);
                                context.Session.SetString("LastTrackTime", DateTime.UtcNow.ToString("O"));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error tracking online user");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnlineUserTrackingMiddleware");
            }

            await _next(context);
        }
    }
}
