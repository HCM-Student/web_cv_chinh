// Controllers/LanguageController.cs
using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace WEB_CV.Controllers
{
    // Dùng route quy ước /Language/SetLanguage để khớp với asp-controller/asp-action trong header
    [Route("[controller]/[action]")]
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult SetLanguage(string culture = "vi-VN", string? returnUrl = "/")
        {
            // QUAN TRỌNG: Path = "/" để cookie áp dụng cho toàn site
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/",                      // ✅ bắt buộc
                    Secure = Request.IsHttps,        // an toàn hơn khi chạy HTTPS
                    HttpOnly = false                 // cho phép client đọc nếu cần
                });

            // Quay về đúng trang trước đó
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }
    }
}
