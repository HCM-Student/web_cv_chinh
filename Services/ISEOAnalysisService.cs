using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface ISEOAnalysisService
    {
        Task<SEOAnalysisVM> AnalyzeBaiVietAsync(int baiVietId);
        Task<SEOAnalysisVM> AnalyzeBaiVietContentAsync(string tieuDe, string? tomTat, string noiDung, string? anhTieuDeAlt);
        Task<SEOAnalysis?> GetLatestAnalysisAsync(int baiVietId);
        Task SaveAnalysisAsync(SEOAnalysis analysis);
        Task<List<string>> GetGoiYCaiThienAsync(int baiVietId);
    }
}
