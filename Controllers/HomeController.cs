using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Models;
using WEB_CV.Data;
using web_cv.Models;

namespace web_cv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NewsDbContext _context;

        public HomeController(ILogger<HomeController> logger, NewsDbContext context)
        {
            _logger = logger;
            _context = context;
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

        // ===== Trang Thiết bị =====
        [HttpGet]
        [Route("Home/ThietBi")]
        [Route("thiet-bi")]
        public IActionResult ThietBi()
        {
            return View("ThietBi");
        }

        // ===== Trang Tuyển dụng =====
        [HttpGet]
        [Route("Home/TuyenDung")]
        [Route("tuyen-dung")]
        public IActionResult TuyenDung()
        {
            return View("TuyenDung");
        }

        // ===== Trang Báo giá =====
        [HttpGet]
        [Route("Home/BaoGia")]
        [Route("bao-gia")]
        public IActionResult BaoGia()
        {
            return View("BaoGia");
        }

        // ===== Trang Liên hệ =====
        [HttpGet]
        [Route("Home/LienHe")]
        [Route("lien-he")]
        public IActionResult LienHe()
        {
            return View("LienHe");
        }

        // ===== Trang Tin tức - Sự kiện =====
        [HttpGet]
        [Route("Home/TinTuc")]
        [Route("tin-tuc")]
        public IActionResult TinTuc()
        {
            // Lấy danh sách bài viết từ database
            var baiViets = _context.BaiViets
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .Include(b => b.BaiVietTags)
                    .ThenInclude(bt => bt.Tag)
                .OrderByDescending(b => b.NgayDang)
                .Take(12) // Lấy 12 bài mới nhất
                .ToList();
                
            return View("TinTuc", baiViets);
        }

        // ===== Chi tiết bài viết =====
        [HttpGet]
        [Route("Home/ChiTietBaiViet/{id}")]
        [Route("bai-viet/{id}")]
        public IActionResult ChiTietBaiViet(int id)
        {
            var baiViet = _context.BaiViets
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .Include(b => b.BaiVietTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefault(b => b.Id == id);
                
            if (baiViet == null)
            {
                return NotFound();
            }
            
            return View("ChiTietBaiViet", baiViet);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
