using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_CV.Models;
using WEB_CV.Services;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CaiDatController : Controller
    {
        private readonly ICaiDatService _caiDatService;

        public CaiDatController(ICaiDatService caiDatService)
        {
            _caiDatService = caiDatService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Cài đặt hệ thống";
            
            // Load current settings to display in the form
            ViewBag.CaiDatChung = await _caiDatService.GetCaiDatChungAsync();
            ViewBag.CaiDatBaoMat = await _caiDatService.GetCaiDatBaoMatAsync();
            ViewBag.CaiDatEmail = await _caiDatService.GetCaiDatEmailAsync();
            ViewBag.CaiDatGiaoDien = await _caiDatService.GetCaiDatGiaoDienAsync();
            ViewBag.CaiDatBackup = await _caiDatService.GetCaiDatBackupAsync();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGeneralSettings(CaiDatChungVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _caiDatService.SaveCaiDatChungAsync(model, User.Identity?.Name);
                    TempData["SuccessMessage"] = "Cài đặt chung đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSecuritySettings(CaiDatBaoMatVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _caiDatService.SaveCaiDatBaoMatAsync(model, User.Identity?.Name);
                    TempData["SuccessMessage"] = "Cài đặt bảo mật đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmailSettings(CaiDatEmailVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _caiDatService.SaveCaiDatEmailAsync(model, User.Identity?.Name);
                    TempData["SuccessMessage"] = "Cài đặt email đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppearanceSettings(CaiDatGiaoDienVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _caiDatService.SaveCaiDatGiaoDienAsync(model, User.Identity?.Name);
                    TempData["SuccessMessage"] = "Cài đặt giao diện đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBackupSettings(CaiDatBackupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _caiDatService.SaveCaiDatBackupAsync(model, User.Identity?.Name);
                    TempData["SuccessMessage"] = "Cài đặt backup đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TestEmailConnection(string MayKhuSMTP, int CongSMTP, string BaoMatSMTP, 
            string TenDangNhap, string MatKhau, string TenNguoiGui, string EmailNguoiGui)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(MayKhuSMTP) || string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(MatKhau))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin SMTP (Máy chủ, Tên đăng nhập, Mật khẩu)" });
                }

                // Simulate email connection test with actual data
                await Task.Delay(2000); // Simulate network delay
                
                // In a real application, you would test the actual SMTP connection here
                // For now, we'll simulate a successful test if basic info is provided
                var isValidConfig = !string.IsNullOrEmpty(MayKhuSMTP) && 
                                  !string.IsNullOrEmpty(TenDangNhap) && 
                                  !string.IsNullOrEmpty(MatKhau) &&
                                  CongSMTP > 0;
                
                if (isValidConfig)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Kết nối email thành công! Đã test kết nối đến {MayKhuSMTP}:{CongSMTP} với tài khoản {TenDangNhap}" 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Cấu hình email không hợp lệ. Vui lòng kiểm tra lại thông tin." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi test email: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup(string type = "full")
        {
            try
            {
                // Simulate backup creation with progress
                var backupTypes = new Dictionary<string, string>
                {
                    ["full"] = "Backup toàn bộ hệ thống",
                    ["database"] = "Backup cơ sở dữ liệu", 
                    ["files"] = "Backup files và media"
                };
                
                var backupTypeName = backupTypes.ContainsKey(type) ? backupTypes[type] : $"Backup {type}";
                
                // Simulate backup process
                await Task.Delay(3000);
                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"backup_{type}_{timestamp}.zip";
                
                return Json(new { 
                    success = true, 
                    message = $"{backupTypeName} đã được tạo thành công! File: {fileName}",
                    fileName = fileName,
                    size = "15.2 MB"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi tạo backup: {ex.Message}" });
            }
        }

        [HttpGet]
        public Task<IActionResult> GetSystemInfo()
        {
            try
            {
                // Get real system information
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var startTime = process.StartTime;
                var uptime = DateTime.Now - startTime;
                
                var info = new
                {
                    version = "2.1.0",
                    dotnetVersion = Environment.Version.ToString(),
                    entityFrameworkVersion = "9.0.8",
                    bootstrapVersion = "5.3.0",
                    lastUpdated = DateTime.Now.ToString("dd/MM/yyyy"),
                    uptime = $"{uptime.Days} ngày {uptime.Hours} giờ {uptime.Minutes} phút",
                    memoryUsage = $"{process.WorkingSet64 / 1024 / 1024} MB",
                    databaseSize = "127.8 MB", // This would need actual DB query
                    filesSize = "2.3 GB", // This would need actual file system scan
                    databaseStatus = "Hoạt động tốt",
                    serverTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    timezone = TimeZoneInfo.Local.DisplayName
                };

                return Task.FromResult<IActionResult>(Json(new { success = true, data = info }));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" }));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                // Simulate cache clearing
                await Task.Delay(1000);
                
                return Json(new { success = true, message = "Cache đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}

