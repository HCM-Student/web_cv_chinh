using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_CV.Services;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,TruongPhongPhatTrien")]
    public class SEOController : Controller
    {
        private readonly ISEOAnalysisService _seoService;

        public SEOController(ISEOAnalysisService seoService)
        {
            _seoService = seoService;
        }

        // GET: /Admin/SEO/Analyze/5
        public async Task<IActionResult> Analyze(int id)
        {
            try
            {
                var analysis = await _seoService.AnalyzeBaiVietAsync(id);
                return Json(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Admin/SEO/AnalyzeContent
        [HttpPost]
        public async Task<IActionResult> AnalyzeContent([FromBody] AnalyzeContentRequest request)
        {
            try
            {
                var analysis = await _seoService.AnalyzeBaiVietContentAsync(
                    request.TieuDe, 
                    request.TomTat, 
                    request.NoiDung, 
                    request.AnhTieuDeAlt
                );
                return Json(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Admin/SEO/GetAnalysis/5
        public async Task<IActionResult> GetAnalysis(int id)
        {
            try
            {
                var analysis = await _seoService.GetLatestAnalysisAsync(id);
                return Json(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Admin/SEO/GetSuggestions/5
        public async Task<IActionResult> GetSuggestions(int id)
        {
            try
            {
                var suggestions = await _seoService.GetGoiYCaiThienAsync(id);
                return Json(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AnalyzeContentRequest
    {
        public string TieuDe { get; set; } = string.Empty;
        public string? TomTat { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string? AnhTieuDeAlt { get; set; }
    }
}
