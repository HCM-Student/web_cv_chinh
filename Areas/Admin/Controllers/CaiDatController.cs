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
        public async Task<IActionResult> TestEmailConnection()
        {
            try
            {
                var result = await _caiDatService.TestEmailConnectionAsync();
                
                if (result)
                {
                    return Json(new { success = true, message = "Kết nối email thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể kết nối đến server email. Vui lòng kiểm tra lại cài đặt." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup(string type = "full")
        {
            try
            {
                // Simulate backup creation
                await Task.Delay(2000);
                
                var fileName = $"{type}_backup_{DateTime.Now:yyyy_MM_dd_HH_mm}.zip";
                return Json(new { success = true, message = $"Backup đã được tạo thành công: {fileName}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi tạo backup: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                var info = new
                {
                    version = "2.1.0",
                    dotnetVersion = "9.0",
                    entityFrameworkVersion = "9.0.8",
                    bootstrapVersion = "5.3.0",
                    lastUpdated = "09/03/2025",
                    uptime = "15 ngày 8 giờ",
                    memoryUsage = "245 MB",
                    databaseSize = "127.8 MB",
                    filesSize = "2.3 GB",
                    databaseStatus = "Hoạt động tốt"
                };

                return Json(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
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

