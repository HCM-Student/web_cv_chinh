using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;
using web_cv.Models;

namespace web_cv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NewsDbContext _db;

        public HomeController(ILogger<HomeController> logger, NewsDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // ===== Trang chủ: trả 3 bài mới nhất làm model =====
             
        [HttpGet]
        public async Task<IActionResult> Index()
{
    var latest3 = await _db.BaiViets
        .Include(x => x.ChuyenMuc)
        .OrderByDescending(x => x.NgayDang)
        .Take(3)
        .AsNoTracking()
        .ToListAsync();

    return View(latest3);   // QUAN TRỌNG: trả model
}


        // ===== Trang Giới thiệu =====
        [HttpGet]
        [Route("Home/GioiThieu")]
        [Route("gioi-thieu")]
        public IActionResult GioiThieu() => View("GioiThieu");

        public IActionResult Privacy() => View();

        // ===== Trang Thiết bị =====
        [HttpGet]
        [Route("Home/ThietBi")]
        [Route("thiet-bi")]
        public IActionResult ThietBi() => View("ThietBi");

        // ===== Trang Tuyển dụng =====
        [HttpGet]
        [Route("Home/TuyenDung")]
        [Route("tuyen-dung")]
        public IActionResult TuyenDung() => View("TuyenDung");

        // ===== Trang Báo giá =====
        [HttpGet]
        [Route("Home/BaoGia")]
        [Route("bao-gia")]
        public IActionResult BaoGia() => View("BaoGia");

        // ===== Trang Liên hệ =====
        [HttpGet]
        [Route("Home/LienHe")]
        [Route("lien-he")]
        public IActionResult LienHe() => View("LienHe");

        // ===== Trang Tin tức - Sự kiện (public) =====
        [HttpGet]
        [Route("Home/TinTuc")]
        [Route("tin-tuc")]
        public async Task<IActionResult> TinTuc()
        {
            var posts = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            // View công khai: Views/TinTuc/Index.cshtml
            return View("~/Views/TinTuc/Index.cshtml", posts);
        }

        // ===== Chi tiết bài viết (public) =====
        [HttpGet]
        [Route("Home/ChiTietBaiViet/{id:int}")]
        [Route("bai-viet/{id:int}")]
        public async Task<IActionResult> ChiTietBaiViet(int id)
        {
            var post = await _db.BaiViets
                .AsNoTracking()
                .Include(b => b.ChuyenMuc)
                .Include(b => b.TacGia)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (post == null) return NotFound();

            // View công khai: Views/TinTuc/Details.cshtml
            return View("~/Views/TinTuc/Details.cshtml", post);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
