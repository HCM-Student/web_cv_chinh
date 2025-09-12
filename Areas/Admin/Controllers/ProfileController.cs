using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly NewsDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IPasswordHasher<NguoiDung> _passwordHasher;

        public ProfileController(NewsDbContext db, IWebHostEnvironment env, IPasswordHasher<NguoiDung> passwordHasher)
        {
            _db = db;
            _env = env;
            _passwordHasher = passwordHasher;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            // Lấy email từ ClaimTypes.Email thay vì Name
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return NotFound();
            }

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                VaiTro = user.VaiTro,
                NgayTao = user.NgayTao,
                Avatar = user.Avatar
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return NotFound();
            }

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileEditViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                VaiTro = user.VaiTro,
                Avatar = user.Avatar
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return NotFound();
            }

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return NotFound();
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;

            _db.Update(user);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Cập nhật thông tin thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            // Kiểm tra mật khẩu hiện tại
            var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, model.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng" });
            }

            // Cập nhật mật khẩu mới
            user.MatKhauHash = _passwordHasher.HashPassword(user, model.NewPassword);
            _db.Update(user);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        // POST: Profile/ChangeAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file ảnh" });
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif, .webp)" });
            }

            // Kiểm tra kích thước file (max 5MB)
            if (avatar.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File ảnh không được vượt quá 5MB" });
            }

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            try
            {
                // Tạo thư mục avatars nếu chưa có
                var avatarsPath = Path.Combine(_env.WebRootPath, "img", "avatars");
                if (!Directory.Exists(avatarsPath))
                {
                    Directory.CreateDirectory(avatarsPath);
                }

                // Tạo tên file unique
                var fileName = $"avatar_{user.Id}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(avatarsPath, fileName);

                // Xóa avatar cũ nếu có
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldAvatarPath = Path.Combine(_env.WebRootPath, user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldAvatarPath))
                    {
                        System.IO.File.Delete(oldAvatarPath);
                    }
                }

                // Lưu file mới
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn avatar trong database
                user.Avatar = $"/img/avatars/{fileName}";
                _db.Update(user);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Đổi avatar thành công!", avatar = user.Avatar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lưu file: {ex.Message}" });
            }
        }
    }

    // ViewModels
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string? Avatar { get; set; }
    }

    public class ProfileEditViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
