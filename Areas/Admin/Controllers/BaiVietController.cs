using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BaiVietController : Controller
    {
        // GET: /Admin/BaiViet
        public IActionResult Index()
        {
            // TODO: sau này bind dữ liệu thật từ DbContext
            return View();
        }
    }
}
