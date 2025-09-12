using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Models.Account;

namespace WEB_CV.Controllers
{
    public class AccountController : Controller
    {
        private readonly NewsDbContext _db;
        private readonly IPasswordHasher<NguoiDung> _hasher;

        public AccountController(NewsDbContext db, IPasswordHasher<NguoiDung> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVM { ReturnUrl = returnUrl });

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(x => x.Email == vm.Email && x.KichHoat);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại hoặc đã bị khóa.");
                return View(vm);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.MatKhauHash, vm.MatKhau);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
                return View(vm);
            }

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = vm.GhiNho });

            // Ưu tiên quay lại trang cũ nếu local
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return LocalRedirect(vm.ReturnUrl);

            // 👉 Admin: bay thẳng vào khu vực quản trị (Bài Viết). Muốn vào Dashboard thì đổi controller = "Dashboard"
            if (string.Equals(user.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "BaiViet", new { area = "Admin" });

            // User thường
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var exists = await _db.NguoiDungs.AnyAsync(x => x.Email == vm.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Email), "Email đã tồn tại.");
                return View(vm);
            }

            var user = new NguoiDung
            {
                HoTen = vm.HoTen,
                Email = vm.Email,
                VaiTro = "User",
                KichHoat = true
            };
            user.MatKhauHash = _hasher.HashPassword(user, vm.MatKhau);

            _db.NguoiDungs.Add(user);
            await _db.SaveChangesAsync();

            // Đăng nhập luôn
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
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

            return View(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
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

            var model = new ProfileEditVM
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                Avatar = user.Avatar
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditVM model, IFormFile? avatarFile, bool removeAvatar = false)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
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

            // Kiểm tra email trùng lặp (ngoại trừ user hiện tại)
            if (model.Email != user.Email)
            {
                var existingUser = await _db.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != user.Id);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(model);
                }
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            
            // Xử lý avatar
            if (removeAvatar)
            {
                // Xóa avatar hiện tại
                user.Avatar = null;
            }
            else if (avatarFile != null && avatarFile.Length > 0)
            {
                // Xóa avatar cũ nếu có
                if (!string.IsNullOrEmpty(user.Avatar) && user.Avatar.StartsWith("/uploads/avatars/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                // Tạo tên file unique
                var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                
                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                
                // Cập nhật đường dẫn avatar
                user.Avatar = $"/uploads/avatars/{fileName}";
            }

            _db.Update(user);
            await _db.SaveChangesAsync();

            // Cập nhật claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
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

            // Kiểm tra mật khẩu cũ
            var verify = _hasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhauCu);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu cũ không đúng.");
                return View(model);
            }

            // Cập nhật mật khẩu mới
            user.MatKhauHash = _hasher.HashPassword(user, model.MatKhauMoi);
            _db.Update(user);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }
    }

    // ViewModels
    public class ProfileEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string Email { get; set; } = string.Empty;

        public string? Avatar { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        [DataType(DataType.Password)]
        public string MatKhauCu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
