using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface ICaiDatService
    {
        // Get settings
        Task<string> GetSettingAsync(string key, string defaultValue = "");
        Task<T> GetSettingAsync<T>(string key, T defaultValue = default!);
        Task<Dictionary<string, string>> GetAllSettingsAsync();

        // Set settings
        Task SetSettingAsync(string key, string value, string? description = null);
        Task SetSettingAsync<T>(string key, T value, string? description = null);

        // Bulk operations
        Task SetMultipleSettingsAsync(Dictionary<string, string> settings, string? updatedBy = null);
        Task<bool> DeleteSettingAsync(string key);

        // ViewModels
        Task<CaiDatChungVM> GetCaiDatChungAsync();
        Task SaveCaiDatChungAsync(CaiDatChungVM model, string? updatedBy = null);

        Task<CaiDatBaoMatVM> GetCaiDatBaoMatAsync();
        Task SaveCaiDatBaoMatAsync(CaiDatBaoMatVM model, string? updatedBy = null);

        Task<CaiDatEmailVM> GetCaiDatEmailAsync();
        Task SaveCaiDatEmailAsync(CaiDatEmailVM model, string? updatedBy = null);

        Task<CaiDatGiaoDienVM> GetCaiDatGiaoDienAsync();
        Task SaveCaiDatGiaoDienAsync(CaiDatGiaoDienVM model, string? updatedBy = null);

        Task<CaiDatBackupVM> GetCaiDatBackupAsync();
        Task SaveCaiDatBackupAsync(CaiDatBackupVM model, string? updatedBy = null);

        // Utilities
        Task InitializeDefaultSettingsAsync();
        Task<bool> TestEmailConnectionAsync();
    }
}
