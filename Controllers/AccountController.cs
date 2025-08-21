using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var user = await _db.NguoiDungs.FirstOrDefaultAsync(x => x.Email == vm.Email && x.KichHoat);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại hoặc đã bị khóa.");
                return View(vm);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.MatKhauHash, vm.MatKhau);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = vm.GhiNho });

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return user.VaiTro == "Admin"
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
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
                new Claim(ClaimTypes.Name, user.HoTen),
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
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
