using WEB_CV.Models;
using System.Text.Json;
using System.Text;

namespace WEB_CV.Services
{
    public class AIWritingService : IAIWritingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIWritingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AIWritingService(HttpClient httpClient, ILogger<AIWritingService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AIWritingResponse> GenerateContentAsync(AIWritingRequest request)
        {
            try
            {
                // Lưu request data vào HttpContext để sử dụng trong mock response
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["AIRequest"] = request;
                }

                // Sử dụng OpenAI API hoặc API tương tự
                var prompt = BuildContentPrompt(request);
                var response = await CallAIAPI(prompt, "content");
                
                if (response.Success)
                {
                    var content = ParseAIResponse(response.Content);
                    var seoAnalysis = await AnalyzeContentSEO(content.Title, content.Content, request.Keywords);
                    
                    return new AIWritingResponse
                    {
                        Title = content.Title,
                        Content = content.Content,
                        Summary = content.Summary,
                        Keywords = content.Keywords,
                        SEOAnalysis = seoAnalysis,
                        Success = true
                    };
                }

                return new AIWritingResponse
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung AI");
                return new AIWritingResponse
                {
                    Success = false,
                    ErrorMessage = "Có lỗi xảy ra khi tạo nội dung. Vui lòng thử lại."
                };
            }
        }

        public async Task<SEOAnalysisResponse> AnalyzeSEOAsync(SEOAnalysisRequest request)
        {
            try
            {
                var analysis = await AnalyzeContentSEO(request.Title, request.Content, request.Keywords);
                var issues = AnalyzeSEOIssues(request.Title, request.Content, request.TargetKeyword);
                var suggestions = GenerateSEOSuggestions(request.Title, request.Content, request.TargetKeyword);

                return new SEOAnalysisResponse
                {
                    Score = analysis.Score,
                    Issues = issues,
                    Suggestions = suggestions,
                    Analysis = analysis,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân tích SEO");
                return new SEOAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Có lỗi xảy ra khi phân tích SEO. Vui lòng thử lại."
                };
            }
        }

        public async Task<ContentSuggestionResponse> GetContentSuggestionsAsync(ContentSuggestionRequest request)
        {
            try
            {
                var prompt = BuildSuggestionPrompt(request);
                var response = await CallAIAPI(prompt, "suggestions");
                
                if (response.Success)
                {
                    var suggestions = ParseSuggestionResponse(response.Content);
                    return new ContentSuggestionResponse
                    {
                        TitleSuggestions = suggestions.TitleSuggestions,
                        ContentSuggestions = suggestions.ContentSuggestions,
                        MetaDescriptionSuggestions = suggestions.MetaDescriptionSuggestions,
                        TagSuggestions = suggestions.TagSuggestions,
                        Success = true
                    };
                }

                return new ContentSuggestionResponse
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý nội dung");
                return new ContentSuggestionResponse
                {
                    Success = false,
                    ErrorMessage = "Có lỗi xảy ra khi lấy gợi ý. Vui lòng thử lại."
                };
            }
        }

        public async Task<KeywordSuggestionResponse> GetKeywordSuggestionsAsync(string topic)
        {
            try
            {
                _logger.LogInformation("GetKeywordSuggestionsAsync called with topic: {Topic}", topic);
                
                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("Topic is null or empty");
                    return new KeywordSuggestionResponse
                    {
                        Success = false,
                        ErrorMessage = "Chủ đề không được để trống"
                    };
                }

                var keywords = await GenerateKeywordSuggestions(topic);
                _logger.LogInformation("Generated {Count} keywords", keywords.Count);
                
                return new KeywordSuggestionResponse
                {
                    Keywords = keywords,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý từ khóa cho topic: {Topic}", topic);
                return new KeywordSuggestionResponse
                {
                    Success = false,
                    ErrorMessage = "Có lỗi xảy ra khi lấy gợi ý từ khóa. Vui lòng thử lại."
                };
            }
        }

        private string BuildContentPrompt(AIWritingRequest request)
        {
            var prompt = $@"Bạn là chuyên gia viết nội dung SEO. Tạo bài viết về chủ đề: {request.Topic}

Yêu cầu:
- Tiêu đề: 50-60 ký tự, SEO-friendly
- Nội dung: {request.WordCount}, có cấu trúc với heading H2, H3
- Tóm tắt: 150-200 ký tự
- Từ khóa: {string.Join(", ", request.Keywords)}
- Phong cách: {request.Style}
- Ngôn ngữ: {request.Language}

TRẢ VỀ ĐÚNG FORMAT JSON SAU (không có text khác):
{{
    ""title"": ""Tiêu đề cụ thể về {request.Topic}"",
    ""content"": ""<h2>Phần 1</h2><p>Nội dung chi tiết...</p><h2>Phần 2</h2><p>Nội dung tiếp theo...</p>"",
    ""summary"": ""Tóm tắt ngắn gọn về {request.Topic}"",
    ""keywords"": [""{request.Topic.ToLower()}"", ""từ khóa liên quan 1"", ""từ khóa liên quan 2""]
}}";

            return prompt;
        }

        private string BuildSuggestionPrompt(ContentSuggestionRequest request)
        {
            return $@"
Dựa trên chủ đề: {request.Topic}
Nội dung hiện tại: {request.CurrentContent}
Từ khóa: {string.Join(", ", request.Keywords)}

Hãy đưa ra gợi ý:
1. 5 tiêu đề hấp dẫn khác
2. 5 ý tưởng nội dung để mở rộng
3. 3 meta description SEO
4. 10 tag liên quan

Định dạng JSON:
{{
    ""titleSuggestions"": [""tiêu đề 1"", ""tiêu đề 2""],
    ""contentSuggestions"": [""ý tưởng 1"", ""ý tưởng 2""],
    ""metaDescriptionSuggestions"": [""meta 1"", ""meta 2""],
    ""tagSuggestions"": [""tag 1"", ""tag 2""]
}}";
        }

        private async Task<AIAPIResponse> CallAIAPI(string prompt, string type)
        {
            try
            {
                var apiKey = _configuration["AI:ApiKey"];
                var provider = _configuration["AI:Provider"] ?? "gemini";
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("API Key không được cấu hình! Vui lòng thêm API Key vào appsettings.json");
                    return new AIAPIResponse
                    {
                        Success = false,
                        ErrorMessage = "API Key không được cấu hình. Vui lòng thêm Gemini API Key vào cấu hình."
                    };
                }

                if (provider.ToLower() == "gemini")
                {
                    return await CallGeminiAPI(prompt, apiKey);
                }
                else
                {
                    return await CallOpenAIAPI(prompt, apiKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi AI API");
                return new AIAPIResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi kết nối AI API: " + ex.Message
                };
            }
        }

        private async Task<AIAPIResponse> CallGeminiAPI(string prompt, string apiKey)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = _configuration.GetValue<double>("AI:Temperature", 0.7),
                        maxOutputTokens = _configuration.GetValue<int>("AI:MaxTokens", 2000),
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Gemini API response: {Response}", responseContent);
                    
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                    
                    var generatedText = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? "";
                    _logger.LogInformation("Generated text from Gemini: {Text}", generatedText);
                    
                    return new AIAPIResponse
                    {
                        Success = true,
                        Content = generatedText
                    };
                }

                _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return new AIAPIResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi Gemini API: " + response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini API");
                return new AIAPIResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi kết nối Gemini API: " + ex.Message
                };
            }
        }

        private async Task<AIAPIResponse> CallOpenAIAPI(string prompt, string apiKey)
        {
            try
            {
                var requestBody = new
                {
                    model = _configuration["AI:Model"] ?? "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _configuration.GetValue<int>("AI:MaxTokens", 2000),
                    temperature = _configuration.GetValue<double>("AI:Temperature", 0.7)
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                    
                    return new AIAPIResponse
                    {
                        Success = true,
                        Content = aiResponse?.choices?.FirstOrDefault()?.message?.content ?? ""
                    };
                }

                return new AIAPIResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi OpenAI API: " + response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi OpenAI API");
                return new AIAPIResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi kết nối OpenAI API: " + ex.Message
                };
            }
        }


        private (string Title, string Content, string Summary, string[] Keywords) ParseAIResponse(string jsonResponse)
        {
            try
            {
                _logger.LogInformation("Parsing AI response: {Response}", jsonResponse);
                
                // Thử parse JSON trực tiếp
                var response = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                
                var title = response.GetProperty("title").GetString() ?? "";
                var content = response.GetProperty("content").GetString() ?? "";
                var summary = response.GetProperty("summary").GetString() ?? "";
                var keywords = response.GetProperty("keywords").EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
                
                _logger.LogInformation("Parsed successfully - Title: {Title}, Content length: {ContentLength}, Summary: {Summary}, Keywords: {Keywords}", 
                    title, content.Length, summary, string.Join(", ", keywords));
                
                return (title, content, summary, keywords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse JSON, trying to extract from text: {Response}", jsonResponse);
                
                // Fallback: Thử extract từ text thô
                try
                {
                    var lines = jsonResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var title = lines.FirstOrDefault(l => l.Contains("title") || l.Contains("Tiêu đề"))?.Split(':').LastOrDefault()?.Trim().Trim('"', '\'') ?? "";
                    var content = lines.FirstOrDefault(l => l.Contains("content") || l.Contains("Nội dung"))?.Split(':').LastOrDefault()?.Trim().Trim('"', '\'') ?? "";
                    var summary = lines.FirstOrDefault(l => l.Contains("summary") || l.Contains("Tóm tắt"))?.Split(':').LastOrDefault()?.Trim().Trim('"', '\'') ?? "";
                    
                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
                    {
                        _logger.LogInformation("Extracted from text - Title: {Title}, Content length: {ContentLength}", title, content.Length);
                        return (title, content, summary, new[] { "từ khóa liên quan" });
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Failed to extract from text");
                }
                
                // Cuối cùng trả về dữ liệu mẫu
                _logger.LogError("All parsing methods failed, returning sample data");
                return ("Tiêu đề mẫu", "Nội dung mẫu", "Tóm tắt mẫu", new[] { "từ khóa 1", "từ khóa 2" });
            }
        }

        private (List<string> TitleSuggestions, List<string> ContentSuggestions, List<string> MetaDescriptionSuggestions, List<string> TagSuggestions) ParseSuggestionResponse(string jsonResponse)
        {
            try
            {
                var response = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                return (
                    response.GetProperty("titleSuggestions").EnumerateArray().Select(x => x.GetString() ?? "").ToList(),
                    response.GetProperty("contentSuggestions").EnumerateArray().Select(x => x.GetString() ?? "").ToList(),
                    response.GetProperty("metaDescriptionSuggestions").EnumerateArray().Select(x => x.GetString() ?? "").ToList(),
                    response.GetProperty("tagSuggestions").EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                );
            }
            catch
            {
                return (new List<string>(), new List<string>(), new List<string>(), new List<string>());
            }
        }

        private Task<AISEOAnalysis> AnalyzeContentSEO(string title, string content, string[] keywords)
        {
            var analysis = new AISEOAnalysis
            {
                TitleLength = title.Length,
                ContentLength = content.Length,
                KeywordDensity = CalculateKeywordDensity(content, keywords),
                ReadabilityScore = CalculateReadabilityScore(content),
                MetaDescriptionLength = 0, // Sẽ được cập nhật sau
                HasImages = content.Contains("<img") || content.Contains("![")
            };

            // Tính điểm SEO
            var score = 0;
            if (analysis.TitleLength >= 30 && analysis.TitleLength <= 60) score += 20;
            if (analysis.ContentLength >= 300) score += 20;
            if (analysis.KeywordDensity > 0.01 && analysis.KeywordDensity < 0.03) score += 20;
            if (analysis.ReadabilityScore > 60) score += 20;
            if (analysis.HasImages) score += 10;
            if (keywords.Length > 0) score += 10;

            analysis.Score = Math.Min(score, 100);
            return Task.FromResult(analysis);
        }

        private List<SEOIssue> AnalyzeSEOIssues(string title, string content, string targetKeyword)
        {
            var issues = new List<SEOIssue>();

            if (title.Length < 30)
                issues.Add(new SEOIssue { Type = "Title", Message = "Tiêu đề quá ngắn", Severity = "warning", Fix = "Tăng độ dài tiêu đề lên 30-60 ký tự" });

            if (title.Length > 60)
                issues.Add(new SEOIssue { Type = "Title", Message = "Tiêu đề quá dài", Severity = "warning", Fix = "Rút gọn tiêu đề xuống dưới 60 ký tự" });

            if (content.Length < 300)
                issues.Add(new SEOIssue { Type = "Content", Message = "Nội dung quá ngắn", Severity = "error", Fix = "Tăng độ dài nội dung lên ít nhất 300 từ" });

            if (!string.IsNullOrEmpty(targetKeyword) && !title.ToLower().Contains(targetKeyword.ToLower()))
                issues.Add(new SEOIssue { Type = "Keyword", Message = "Từ khóa chính không có trong tiêu đề", Severity = "error", Fix = "Thêm từ khóa chính vào tiêu đề" });

            return issues;
        }

        private List<SEOSuggestion> GenerateSEOSuggestions(string title, string content, string targetKeyword)
        {
            var suggestions = new List<SEOSuggestion>();

            suggestions.Add(new SEOSuggestion
            {
                Type = "Structure",
                Message = "Thêm heading H2, H3 để cải thiện cấu trúc",
                Priority = "high",
                Implementation = "Sử dụng thẻ <h2> và <h3> để chia nhỏ nội dung"
            });

            suggestions.Add(new SEOSuggestion
            {
                Type = "Images",
                Message = "Thêm hình ảnh có alt text",
                Priority = "medium",
                Implementation = "Chèn hình ảnh liên quan với thuộc tính alt mô tả"
            });

            suggestions.Add(new SEOSuggestion
            {
                Type = "Links",
                Message = "Thêm liên kết nội bộ và ngoại bộ",
                Priority = "medium",
                Implementation = "Liên kết đến các bài viết liên quan và nguồn uy tín"
            });

            return suggestions;
        }

        private Task<List<KeywordSuggestion>> GenerateKeywordSuggestions(string topic)
        {
            // Mock data - trong thực tế sẽ gọi API keyword research
            return Task.FromResult(new List<KeywordSuggestion>
            {
                new() { Keyword = topic, SearchVolume = 1000, Difficulty = 50, CPC = 1.5, RelatedKeywords = new[] { topic + " 2024", topic + " mới nhất" } },
                new() { Keyword = topic + " hướng dẫn", SearchVolume = 500, Difficulty = 30, CPC = 1.2, RelatedKeywords = new[] { "cách " + topic, topic + " tips" } },
                new() { Keyword = topic + " công nghệ", SearchVolume = 800, Difficulty = 40, CPC = 1.8, RelatedKeywords = new[] { topic + " AI", topic + " digital" } }
            });
        }

        private double CalculateKeywordDensity(string content, string[] keywords)
        {
            if (keywords.Length == 0) return 0;
            
            var totalWords = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var keywordCount = keywords.Sum(keyword => 
                content.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Count(word => word.Contains(keyword.ToLower())));
            
            return totalWords > 0 ? (double)keywordCount / totalWords : 0;
        }

        private int CalculateReadabilityScore(string content)
        {
            // Simplified readability calculation
            var sentences = content.Split('.', '!', '?').Length;
            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var syllables = content.ToLower().Count(c => "aeiou".Contains(c));
            
            if (sentences == 0 || words == 0) return 0;
            
            var avgWordsPerSentence = (double)words / sentences;
            var avgSyllablesPerWord = (double)syllables / words;
            
            var score = 206.835 - (1.015 * avgWordsPerSentence) - (84.6 * avgSyllablesPerWord);
            return Math.Max(0, Math.Min(100, (int)score));
        }
    }

    public class AIAPIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class OpenAIResponse
    {
        public Choice[]? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? content { get; set; }
    }

    public class GeminiResponse
    {
        public GeminiCandidate[]? candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[]? parts { get; set; }
    }

    public class GeminiPart
    {
        public string? text { get; set; }
    }
}
