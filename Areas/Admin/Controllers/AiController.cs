using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_CV.Services.AI;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class AiController : Controller
    {
        private readonly IProtonxSearch _ai;
        public AiController(IProtonxSearch ai) => _ai = ai;

        // GET /Admin/Ai   (demo page)
        public IActionResult Index() => View();

        // POST /Admin/Ai/Search
        [HttpPost]
        public async Task<IActionResult> Search([FromForm] string q, [FromForm] int n = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return Json(new { error = "Query không được để trống" });
                }
                
                var hits = await _ai.SearchAsync(q, n);
                return Json(new { results = hits });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
