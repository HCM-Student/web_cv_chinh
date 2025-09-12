using Microsoft.AspNetCore.Mvc;
using WEB_CV.Services;
using WEB_CV.Models;

namespace WEB_CV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIWritingService _aiService;
        private readonly ILogger<AIController> _logger;
        private readonly IConfiguration _configuration;

        public AIController(IAIWritingService aiService, ILogger<AIController> logger, IConfiguration configuration)
        {
            _aiService = aiService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo nội dung bài viết bằng AI
        /// </summary>
        [HttpPost("generate-content")]
        public async Task<IActionResult> GenerateContent([FromBody] AIWritingRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Topic))
                {
                    return BadRequest(new { success = false, message = "Chủ đề không được để trống" });
                }

                var result = await _aiService.GenerateContentAsync(request);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            title = result.Title,
                            content = result.Content,
                            summary = result.Summary,
                            keywords = result.Keywords,
                            seoAnalysis = result.SEOAnalysis
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung AI");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi tạo nội dung" });
            }
        }

        /// <summary>
        /// Phân tích SEO cho nội dung
        /// </summary>
        [HttpPost("analyze-seo")]
        public async Task<IActionResult> AnalyzeSEO([FromBody] SEOAnalysisRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest(new { success = false, message = "Tiêu đề hoặc nội dung không được để trống" });
                }

                var result = await _aiService.AnalyzeSEOAsync(request);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            score = result.Score,
                            issues = result.Issues,
                            suggestions = result.Suggestions,
                            analysis = result.Analysis
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân tích SEO");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi phân tích SEO" });
            }
        }

        /// <summary>
        /// Lấy gợi ý nội dung
        /// </summary>
        [HttpPost("content-suggestions")]
        public async Task<IActionResult> GetContentSuggestions([FromBody] ContentSuggestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Topic))
                {
                    return BadRequest(new { success = false, message = "Chủ đề không được để trống" });
                }

                var result = await _aiService.GetContentSuggestionsAsync(request);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            titleSuggestions = result.TitleSuggestions,
                            contentSuggestions = result.ContentSuggestions,
                            metaDescriptionSuggestions = result.MetaDescriptionSuggestions,
                            tagSuggestions = result.TagSuggestions
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý nội dung");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi lấy gợi ý" });
            }
        }

        /// <summary>
        /// Lấy gợi ý từ khóa
        /// </summary>
        [HttpGet("keyword-suggestions")]
        public async Task<IActionResult> GetKeywordSuggestions([FromQuery] string topic)
        {
            try
            {
                _logger.LogInformation("GetKeywordSuggestions called with topic: {Topic}", topic);
                
                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("Topic is null or empty in controller");
                    return BadRequest(new { success = false, message = "Chủ đề không được để trống" });
                }

                var result = await _aiService.GetKeywordSuggestionsAsync(topic);
                _logger.LogInformation("Service returned success: {Success}, keywords count: {Count}", result.Success, result.Keywords?.Count ?? 0);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            keywords = result.Keywords
                        }
                    });
                }

                _logger.LogWarning("Service returned error: {Error}", result.ErrorMessage);
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý từ khóa cho topic: {Topic}", topic);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi lấy gợi ý từ khóa" });
            }
        }

        /// <summary>
        /// Tối ưu hóa nội dung cho SEO
        /// </summary>
        [HttpPost("optimize-content")]
        public async Task<IActionResult> OptimizeContent([FromBody] ContentOptimizationRequest request)
        {
            try
            {
                var seoResult = await _aiService.AnalyzeSEOAsync(new SEOAnalysisRequest
                {
                    Title = request.Title,
                    Content = request.Content,
                    Keywords = request.Keywords,
                    TargetKeyword = request.TargetKeyword
                });

                var suggestions = await _aiService.GetContentSuggestionsAsync(new ContentSuggestionRequest
                {
                    Topic = request.Topic,
                    CurrentContent = request.Content,
                    Keywords = request.Keywords
                });

                if (seoResult.Success && suggestions.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            seoAnalysis = seoResult.Analysis,
                            seoScore = seoResult.Score,
                            issues = seoResult.Issues,
                            suggestions = seoResult.Suggestions,
                            contentSuggestions = suggestions.ContentSuggestions,
                            titleSuggestions = suggestions.TitleSuggestions,
                            metaSuggestions = suggestions.MetaDescriptionSuggestions,
                            tagSuggestions = suggestions.TagSuggestions
                        }
                    });
                }

                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi tối ưu hóa nội dung" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tối ưu hóa nội dung");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi tối ưu hóa nội dung" });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái AI provider
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetAIStatus()
        {
            try
            {
                var apiKey = _configuration["AI:ApiKey"];
                var provider = _configuration["AI:Provider"] ?? "gemini";
                
                return Ok(new
                {
                    provider = provider,
                    hasApiKey = !string.IsNullOrEmpty(apiKey),
                    model = _configuration["AI:Model"] ?? "gemini-1.5-flash"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy trạng thái AI");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi kiểm tra trạng thái AI" });
            }
        }
    }

    public class ContentOptimizationRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string TargetKeyword { get; set; } = string.Empty;
    }
}
