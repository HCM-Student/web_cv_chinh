using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface IAIWritingService
    {
        Task<AIWritingResponse> GenerateContentAsync(AIWritingRequest request);
        Task<SEOAnalysisResponse> AnalyzeSEOAsync(SEOAnalysisRequest request);
        Task<ContentSuggestionResponse> GetContentSuggestionsAsync(ContentSuggestionRequest request);
        Task<KeywordSuggestionResponse> GetKeywordSuggestionsAsync(string topic);
    }

    public class AIWritingRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Style { get; set; } = "professional"; // professional, casual, academic, creative
        public int WordCount { get; set; } = 500;
        public string Language { get; set; } = "vi";
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string Tone { get; set; } = "neutral"; // neutral, positive, negative, informative
    }

    public class AIWritingResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public AISEOAnalysis SEOAnalysis { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class SEOAnalysisRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string TargetKeyword { get; set; } = string.Empty;
    }

    public class SEOAnalysisResponse
    {
        public int Score { get; set; }
        public List<SEOIssue> Issues { get; set; } = new();
        public List<SEOSuggestion> Suggestions { get; set; } = new();
        public AISEOAnalysis Analysis { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ContentSuggestionRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string CurrentContent { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
    }

    public class ContentSuggestionResponse
    {
        public List<string> TitleSuggestions { get; set; } = new();
        public List<string> ContentSuggestions { get; set; } = new();
        public List<string> MetaDescriptionSuggestions { get; set; } = new();
        public List<string> TagSuggestions { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class KeywordSuggestionResponse
    {
        public List<KeywordSuggestion> Keywords { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class KeywordSuggestion
    {
        public string Keyword { get; set; } = string.Empty;
        public int SearchVolume { get; set; }
        public int Difficulty { get; set; }
        public double CPC { get; set; }
        public string[] RelatedKeywords { get; set; } = Array.Empty<string>();
    }

    public class SEOIssue
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "warning"; // info, warning, error
        public string Fix { get; set; } = string.Empty;
    }

    public class SEOSuggestion
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = "medium"; // low, medium, high
        public string Implementation { get; set; } = string.Empty;
    }

    public class AISEOAnalysis
    {
        public int TitleLength { get; set; }
        public int ContentLength { get; set; }
        public double KeywordDensity { get; set; }
        public int ReadabilityScore { get; set; }
        public int MetaDescriptionLength { get; set; }
        public bool HasImages { get; set; }
        public int Score { get; set; }
    }
}
