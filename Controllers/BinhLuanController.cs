using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;
using System.Security.Claims;

namespace WEB_CV.Controllers
{
    public class BinhLuanController : Controller
    {
        private readonly NewsDbContext _context;

        public BinhLuanController(NewsDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BinhLuan binhLuan)
        {
            try
            {
                // Kiểm tra validation
                if (string.IsNullOrWhiteSpace(binhLuan.NoiDung))
                {
                    TempData["CommentError"] = "Vui lòng nhập nội dung bình luận.";
                    return RedirectToAction("ChiTietBaiViet", "Home", new { id = binhLuan.BaiVietId });
                }

                // Nếu user đã đăng nhập, lấy thông tin từ claims
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
                        if (user != null)
                        {
                            binhLuan.NguoiDungId = user.Id;
                            binhLuan.HoTen = user.HoTen;
                            binhLuan.Email = user.Email;
                            binhLuan.DaDuyet = true; // User đã đăng nhập thì tự động duyệt
                        }
                    }
                }
                else
                {
                    // Kiểm tra thông tin cho user chưa đăng nhập
                    if (string.IsNullOrWhiteSpace(binhLuan.HoTen))
                    {
                        TempData["CommentError"] = "Vui lòng nhập họ tên.";
                        return RedirectToAction("ChiTietBaiViet", "Home", new { id = binhLuan.BaiVietId });
                    }
                    if (string.IsNullOrWhiteSpace(binhLuan.Email))
                    {
                        TempData["CommentError"] = "Vui lòng nhập email.";
                        return RedirectToAction("ChiTietBaiViet", "Home", new { id = binhLuan.BaiVietId });
                    }
                }

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();
                
                TempData["CommentSuccess"] = "Bình luận của bạn đã được gửi thành công!";
            }
            catch (Exception ex)
            {
                TempData["CommentError"] = "Có lỗi xảy ra khi gửi bình luận. Vui lòng thử lại.";
                // Log error nếu cần
            }

            return RedirectToAction("ChiTietBaiViet", "Home", new { id = binhLuan.BaiVietId });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int baiVietId)
        {
            var comments = await _context.BinhLuans
                .Where(b => b.BaiVietId == baiVietId && b.DaDuyet == true)
                .Include(b => b.NguoiDung)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();

            return PartialView("_CommentsList", comments);
        }
    }
}
