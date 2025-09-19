using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public class CaiDatService : ICaiDatService
    {
        private readonly NewsDbContext _context;
        private readonly ILogger<CaiDatService> _logger;

        public CaiDatService(NewsDbContext context, ILogger<CaiDatService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get single setting
        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            try
            {
                var setting = await _context.CaiDats
                    .FirstOrDefaultAsync(x => x.Key == key);
                
                return setting?.Value ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cài đặt {Key}", key);
                return defaultValue;
            }
        }

        // Get typed setting
        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default!)
        {
            try
            {
                var value = await GetSettingAsync(key);
                
                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)value;
                
                if (typeof(T) == typeof(int))
                    return int.TryParse(value, out var intVal) ? (T)(object)intVal : defaultValue;
                
                if (typeof(T) == typeof(bool))
                    return bool.TryParse(value, out var boolVal) ? (T)(object)boolVal : defaultValue;

                // For complex objects, use JSON
                return JsonSerializer.Deserialize<T>(value) ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cài đặt typed {Key}", key);
                return defaultValue;
            }
        }

        // Get all settings
        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            try
            {
                var settings = await _context.CaiDats
                    .AsNoTracking()
                    .ToDictionaryAsync(x => x.Key, x => x.Value);
                
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả cài đặt");
                return new Dictionary<string, string>();
            }
        }

        // Set single setting
        public async Task SetSettingAsync(string key, string value, string? description = null)
        {
            try
            {
                var setting = await _context.CaiDats
                    .FirstOrDefaultAsync(x => x.Key == key);

                if (setting == null)
                {
                    setting = new CaiDat
                    {
                        Key = key,
                        Value = value,
                        Description = description,
                        Type = "string",
                        NgayCapNhat = DateTime.Now
                    };
                    _context.CaiDats.Add(setting);
                }
                else
                {
                    setting.Value = value;
                    setting.NgayCapNhat = DateTime.Now;
                    if (!string.IsNullOrEmpty(description))
                        setting.Description = description;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu cài đặt {Key}", key);
                throw;
            }
        }

        // Set typed setting
        public async Task SetSettingAsync<T>(string key, T value, string? description = null)
        {
            string stringValue;
            string type = "string";

            if (value is string str)
            {
                stringValue = str;
                type = "string";
            }
            else if (value is int || value is long)
            {
                stringValue = value.ToString() ?? "";
                type = "int";
            }
            else if (value is bool)
            {
                stringValue = value.ToString() ?? "";
                type = "bool";
            }
            else
            {
                stringValue = JsonSerializer.Serialize(value);
                type = "json";
            }

            var setting = await _context.CaiDats
                .FirstOrDefaultAsync(x => x.Key == key);

            if (setting == null)
            {
                setting = new CaiDat
                {
                    Key = key,
                    Value = stringValue,
                    Description = description,
                    Type = type,
                    NgayCapNhat = DateTime.Now
                };
                _context.CaiDats.Add(setting);
            }
            else
            {
                setting.Value = stringValue;
                setting.Type = type;
                setting.NgayCapNhat = DateTime.Now;
                if (!string.IsNullOrEmpty(description))
                    setting.Description = description;
            }

            await _context.SaveChangesAsync();
        }

        // Set multiple settings
        public async Task SetMultipleSettingsAsync(Dictionary<string, string> settings, string? updatedBy = null)
        {
            try
            {
                var keys = settings.Keys.ToList();
                var existingSettings = await _context.CaiDats
                    .Where(x => keys.Contains(x.Key))
                    .ToDictionaryAsync(x => x.Key);

                foreach (var kvp in settings)
                {
                    if (existingSettings.TryGetValue(kvp.Key, out var existing))
                    {
                        existing.Value = kvp.Value;
                        existing.NgayCapNhat = DateTime.Now;
                        existing.NgCapNhatBoi = updatedBy;
                    }
                    else
                    {
                        _context.CaiDats.Add(new CaiDat
                        {
                            Key = kvp.Key,
                            Value = kvp.Value,
                            Type = "string",
                            NgayCapNhat = DateTime.Now,
                            NgCapNhatBoi = updatedBy
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu nhiều cài đặt");
                throw;
            }
        }

        // Delete setting
        public async Task<bool> DeleteSettingAsync(string key)
        {
            try
            {
                var setting = await _context.CaiDats
                    .FirstOrDefaultAsync(x => x.Key == key);

                if (setting != null)
                {
                    _context.CaiDats.Remove(setting);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa cài đặt {Key}", key);
                return false;
            }
        }

        // Get Cai Dat Chung
        public async Task<CaiDatChungVM> GetCaiDatChungAsync()
        {
            return new CaiDatChungVM
            {
                TenWebsite = await GetSettingAsync("TenWebsite", "Cục Chuyển đổi số"),
                Slogan = await GetSettingAsync("Slogan", "Đổi mới số - Phát triển bền vững"),
                MoTaWebsite = await GetSettingAsync("MoTaWebsite", "Website chính thức của Cục Chuyển đổi số"),
                EmailLienHe = await GetSettingAsync("EmailLienHe", "contact@chuyen-doi-so.gov.vn"),
                SoDienThoai = await GetSettingAsync("SoDienThoai", "(024) 1234-5678"),
                MuiGio = await GetSettingAsync("MuiGio", "UTC+7"),
                SoBaiVietMoiTrang = await GetSettingAsync<int>("SoBaiVietMoiTrang", 10),
                DinhDangNgay = await GetSettingAsync("DinhDangNgay", "dd/MM/yyyy"),
                NgonNguMacDinh = await GetSettingAsync("NgonNguMacDinh", "vi")
            };
        }

        // Save Cai Dat Chung
        public async Task SaveCaiDatChungAsync(CaiDatChungVM model, string? updatedBy = null)
        {
            var settings = new Dictionary<string, string>
            {
                ["TenWebsite"] = model.TenWebsite,
                ["Slogan"] = model.Slogan,
                ["MoTaWebsite"] = model.MoTaWebsite,
                ["EmailLienHe"] = model.EmailLienHe,
                ["SoDienThoai"] = model.SoDienThoai,
                ["MuiGio"] = model.MuiGio,
                ["SoBaiVietMoiTrang"] = model.SoBaiVietMoiTrang.ToString(),
                ["DinhDangNgay"] = model.DinhDangNgay,
                ["NgonNguMacDinh"] = model.NgonNguMacDinh
            };

            await SetMultipleSettingsAsync(settings, updatedBy);
        }

        // Get Cai Dat Bao Mat
        public async Task<CaiDatBaoMatVM> GetCaiDatBaoMatAsync()
        {
            return new CaiDatBaoMatVM
            {
                DoDaiMatKhauToiThieu = await GetSettingAsync<int>("DoDaiMatKhauToiThieu", 8),
                ThoiGianHetHanMatKhau = await GetSettingAsync<int>("ThoiGianHetHanMatKhau", 90),
                YeuCauChuHoa = await GetSettingAsync<bool>("YeuCauChuHoa", true),
                YeuCauSo = await GetSettingAsync<bool>("YeuCauSo", true),
                YeuCauKyTuDacBiet = await GetSettingAsync<bool>("YeuCauKyTuDacBiet", true),
                SoLanDangNhapSaiToiDa = await GetSettingAsync<int>("SoLanDangNhapSaiToiDa", 5),
                ThoiGianKhoaTaiKhoan = await GetSettingAsync<int>("ThoiGianKhoaTaiKhoan", 30),
                KichHoatXacThuc2YeuTo = await GetSettingAsync<bool>("KichHoatXacThuc2YeuTo", false),
                GuiThongBaoDangNhapMoi = await GetSettingAsync<bool>("GuiThongBaoDangNhapMoi", true)
            };
        }

        // Save Cai Dat Bao Mat
        public async Task SaveCaiDatBaoMatAsync(CaiDatBaoMatVM model, string? updatedBy = null)
        {
            var settings = new Dictionary<string, string>
            {
                ["DoDaiMatKhauToiThieu"] = model.DoDaiMatKhauToiThieu.ToString(),
                ["ThoiGianHetHanMatKhau"] = model.ThoiGianHetHanMatKhau.ToString(),
                ["YeuCauChuHoa"] = model.YeuCauChuHoa.ToString(),
                ["YeuCauSo"] = model.YeuCauSo.ToString(),
                ["YeuCauKyTuDacBiet"] = model.YeuCauKyTuDacBiet.ToString(),
                ["SoLanDangNhapSaiToiDa"] = model.SoLanDangNhapSaiToiDa.ToString(),
                ["ThoiGianKhoaTaiKhoan"] = model.ThoiGianKhoaTaiKhoan.ToString(),
                ["KichHoatXacThuc2YeuTo"] = model.KichHoatXacThuc2YeuTo.ToString(),
                ["GuiThongBaoDangNhapMoi"] = model.GuiThongBaoDangNhapMoi.ToString()
            };

            await SetMultipleSettingsAsync(settings, updatedBy);
        }

        // Get Cai Dat Email
        public async Task<CaiDatEmailVM> GetCaiDatEmailAsync()
        {
            return new CaiDatEmailVM
            {
                MayKhuSMTP = await GetSettingAsync("MayKhuSMTP", "smtp.gmail.com"),
                CongSMTP = await GetSettingAsync<int>("CongSMTP", 587),
                BaoMatSMTP = await GetSettingAsync("BaoMatSMTP", "TLS"),
                TenDangNhap = await GetSettingAsync("TenDangNhapSMTP", ""),
                MatKhau = await GetSettingAsync("MatKhauSMTP", ""),
                TenNguoiGui = await GetSettingAsync("TenNguoiGui", "Cục Chuyển đổi số"),
                EmailNguoiGui = await GetSettingAsync("EmailNguoiGui", "noreply@chuyen-doi-so.gov.vn"),
                GuiEmailNguoiDungMoi = await GetSettingAsync<bool>("GuiEmailNguoiDungMoi", true),
                ThongBaoBaiVietMoi = await GetSettingAsync<bool>("ThongBaoBaiVietMoi", true),
                ThongBaoPhanHoi = await GetSettingAsync<bool>("ThongBaoPhanHoi", false)
            };
        }

        // Save Cai Dat Email
        public async Task SaveCaiDatEmailAsync(CaiDatEmailVM model, string? updatedBy = null)
        {
            var settings = new Dictionary<string, string>
            {
                ["MayKhuSMTP"] = model.MayKhuSMTP,
                ["CongSMTP"] = model.CongSMTP.ToString(),
                ["BaoMatSMTP"] = model.BaoMatSMTP,
                ["TenDangNhapSMTP"] = model.TenDangNhap,
                ["MatKhauSMTP"] = model.MatKhau,
                ["TenNguoiGui"] = model.TenNguoiGui,
                ["EmailNguoiGui"] = model.EmailNguoiGui,
                ["GuiEmailNguoiDungMoi"] = model.GuiEmailNguoiDungMoi.ToString(),
                ["ThongBaoBaiVietMoi"] = model.ThongBaoBaiVietMoi.ToString(),
                ["ThongBaoPhanHoi"] = model.ThongBaoPhanHoi.ToString()
            };

            await SetMultipleSettingsAsync(settings, updatedBy);
        }

        // Get Cai Dat Giao Dien
        public async Task<CaiDatGiaoDienVM> GetCaiDatGiaoDienAsync()
        {
            return new CaiDatGiaoDienVM
            {
                ChuDeManSac = await GetSettingAsync("ChuDeManSac", "blue"),
                CustomCSS = await GetSettingAsync("CustomCSS", "")
            };
        }

        // Save Cai Dat Giao Dien
        public async Task SaveCaiDatGiaoDienAsync(CaiDatGiaoDienVM model, string? updatedBy = null)
        {
            var settings = new Dictionary<string, string>
            {
                ["ChuDeManSac"] = model.ChuDeManSac,
                ["CustomCSS"] = model.CustomCSS
            };

            await SetMultipleSettingsAsync(settings, updatedBy);
        }

        // Get Cai Dat Backup
        public async Task<CaiDatBackupVM> GetCaiDatBackupAsync()
        {
            return new CaiDatBackupVM
            {
                TanSuatBackup = await GetSettingAsync("TanSuatBackup", "weekly"),
                GioThucHien = await GetSettingAsync("GioThucHien", "02:00"),
                SoLuongBackupGiuLai = await GetSettingAsync<int>("SoLuongBackupGiuLai", 10),
                ThuMucLuuTru = await GetSettingAsync("ThuMucLuuTru", "/backups/")
            };
        }

        // Save Cai Dat Backup
        public async Task SaveCaiDatBackupAsync(CaiDatBackupVM model, string? updatedBy = null)
        {
            var settings = new Dictionary<string, string>
            {
                ["TanSuatBackup"] = model.TanSuatBackup,
                ["GioThucHien"] = model.GioThucHien,
                ["SoLuongBackupGiuLai"] = model.SoLuongBackupGiuLai.ToString(),
                ["ThuMucLuuTru"] = model.ThuMucLuuTru
            };

            await SetMultipleSettingsAsync(settings, updatedBy);
        }

        // Initialize default settings
        public async Task InitializeDefaultSettingsAsync()
        {
            var defaults = new Dictionary<string, string>
            {
                // General settings
                ["TenWebsite"] = "Cục Chuyển đổi số",
                ["Slogan"] = "Đổi mới số - Phát triển bền vững",
                ["MoTaWebsite"] = "Website chính thức của Cục Chuyển đổi số",
                ["EmailLienHe"] = "contact@chuyen-doi-so.gov.vn",
                ["SoDienThoai"] = "(024) 1234-5678",
                ["MuiGio"] = "UTC+7",
                ["SoBaiVietMoiTrang"] = "10",
                ["DinhDangNgay"] = "dd/MM/yyyy",
                ["NgonNguMacDinh"] = "vi",

                // Security settings
                ["DoDaiMatKhauToiThieu"] = "8",
                ["ThoiGianHetHanMatKhau"] = "90",
                ["YeuCauChuHoa"] = "true",
                ["YeuCauSo"] = "true",
                ["YeuCauKyTuDacBiet"] = "true",
                ["SoLanDangNhapSaiToiDa"] = "5",
                ["ThoiGianKhoaTaiKhoan"] = "30",
                ["KichHoatXacThuc2YeuTo"] = "false",
                ["GuiThongBaoDangNhapMoi"] = "true",

                // Email settings
                ["MayKhuSMTP"] = "smtp.gmail.com",
                ["CongSMTP"] = "587",
                ["BaoMatSMTP"] = "TLS",
                ["TenNguoiGui"] = "Cục Chuyển đổi số",
                ["EmailNguoiGui"] = "noreply@chuyen-doi-so.gov.vn",
                ["GuiEmailNguoiDungMoi"] = "true",
                ["ThongBaoBaiVietMoi"] = "true",
                ["ThongBaoPhanHoi"] = "false",

                // Appearance settings
                ["ChuDeManSac"] = "blue",
                ["CustomCSS"] = "",

                // Backup settings
                ["TanSuatBackup"] = "weekly",
                ["GioThucHien"] = "02:00",
                ["SoLuongBackupGiuLai"] = "10",
                ["ThuMucLuuTru"] = "/backups/"
            };

            foreach (var kvp in defaults)
            {
                var exists = await _context.CaiDats.AnyAsync(x => x.Key == kvp.Key);
                if (!exists)
                {
                    await SetSettingAsync(kvp.Key, kvp.Value);
                }
            }
        }

        // Test email connection
        public async Task<bool> TestEmailConnectionAsync()
        {
            try
            {
                // Get email settings
                var settings = await GetCaiDatEmailAsync();
                
                // Test email connection by checking if required settings exist
                await Task.Delay(100); // Brief delay for async operation
                
                return !string.IsNullOrEmpty(settings.MayKhuSMTP) && 
                       !string.IsNullOrEmpty(settings.TenDangNhap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi test kết nối email");
                return false;
            }
        }
    }
}
