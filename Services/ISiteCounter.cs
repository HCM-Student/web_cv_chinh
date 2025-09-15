using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WEB_CV.Services
{
    public interface ISiteCounter
    {
        Task TrackAsync(HttpContext context);   // gọi mỗi request (trừ file tĩnh)
        int GetOnline();                        // số người online (10 phút)
        Task<long> GetTotalVisitsAsync();       // tổng lượt truy cập web
    }
}
