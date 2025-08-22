using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web_cv.Models;

namespace web_cv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ===== Trang Giới thiệu =====
        // Hỗ trợ cả 2 URL: /Home/GioiThieu và /gioi-thieu
        [HttpGet]
        [Route("Home/GioiThieu")]
        [Route("gioi-thieu")]
        public IActionResult GioiThieu()
        {
            // View: Views/Home/GioiThieu.cshtml
            return View("GioiThieu");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
