using WEB_CV.Services;

namespace WEB_CV.Middleware
{
    public class SiteTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        public SiteTrackingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, ISiteCounter counter)
        {
            await counter.TrackAsync(context);
            await _next(context);
        }
    }
}
