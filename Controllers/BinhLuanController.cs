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
                            binhLuan.TrangThai = 1; // User đã đăng nhập thì tự động duyệt
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

                // Xử lý reply - nếu có ParentId thì đây là reply
                if (binhLuan.ParentId.HasValue)
                {
                    // Kiểm tra comment gốc có tồn tại không
                    var parentComment = await _context.BinhLuans
                        .FirstOrDefaultAsync(b => b.Id == binhLuan.ParentId.Value);
                    
                    if (parentComment == null)
                    {
                        TempData["CommentError"] = "Bình luận gốc không tồn tại.";
                        return RedirectToAction("ChiTietBaiViet", "Home", new { id = binhLuan.BaiVietId });
                    }
                    
                    TempData["CommentSuccess"] = "Trả lời của bạn đã được gửi thành công!";
                }
                else
                {
                    TempData["CommentSuccess"] = "Bình luận của bạn đã được gửi thành công!";
                }

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
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
                .Where(b => b.BaiVietId == baiVietId && b.TrangThai == 1 && b.ParentId == null)
                .Include(b => b.NguoiDung)
                .Include(b => b.Replies.Where(r => r.TrangThai == 1))
                    .ThenInclude(r => r.NguoiDung)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();

            return PartialView("_CommentsList", comments);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int parentId, string noiDung, string? hoTen = null, string? email = null)
        {
            try
            {
                // Lấy comment gốc để lấy BaiVietId
                var parentComment = await _context.BinhLuans
                    .FirstOrDefaultAsync(b => b.Id == parentId);
                
                if (parentComment == null)
                {
                    return Json(new { success = false, message = "Bình luận không tồn tại" });
                }

                var reply = new BinhLuan
                {
                    NoiDung = noiDung,
                    BaiVietId = parentComment.BaiVietId,
                    ParentId = parentId,
                    Ngay = DateTime.Now,
                    TrangThai = 0 // Mặc định chờ duyệt
                };

                // Nếu user đã đăng nhập, lấy thông tin từ claims
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
                        if (user != null)
                        {
                            reply.NguoiDungId = user.Id;
                            reply.HoTen = user.HoTen;
                            reply.Email = user.Email;
                            reply.TrangThai = 1; // User đã đăng nhập thì tự động duyệt
                        }
                    }
                }
                else
                {
                    // Kiểm tra thông tin cho user chưa đăng nhập
                    if (string.IsNullOrWhiteSpace(hoTen))
                    {
                        return Json(new { success = false, message = "Vui lòng nhập họ tên" });
                    }
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return Json(new { success = false, message = "Vui lòng nhập email" });
                    }
                    
                    reply.HoTen = hoTen;
                    reply.Email = email;
                }

                _context.BinhLuans.Add(reply);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Trả lời đã được gửi thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi trả lời. Vui lòng thử lại." });
            }
        }

        // Quản lý bình luận của người dùng
        [HttpGet]
        public async Task<IActionResult> MyComments()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comments = await _context.BinhLuans
                .Where(b => b.NguoiDungId == user.Id)
                .Include(b => b.BaiViet)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();

            return View(comments);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.BinhLuans
                .Include(b => b.BaiViet)
                .FirstOrDefaultAsync(b => b.Id == id && b.NguoiDungId == user.Id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BinhLuan binhLuan)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.BinhLuans
                .FirstOrDefaultAsync(b => b.Id == id && b.NguoiDungId == user.Id);

            if (comment == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(binhLuan.NoiDung))
            {
                ModelState.AddModelError("NoiDung", "Vui lòng nhập nội dung bình luận.");
                return View(comment);
            }

            comment.NoiDung = binhLuan.NoiDung;
            comment.TrangThai = 0; // Đặt lại trạng thái chờ duyệt sau khi sửa

            await _context.SaveChangesAsync();
            TempData["CommentSuccess"] = "Bình luận đã được cập nhật và đang chờ phê duyệt!";

            return RedirectToAction("MyComments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.BinhLuans
                .FirstOrDefaultAsync(b => b.Id == id && b.NguoiDungId == user.Id);

            if (comment == null)
            {
                return NotFound();
            }

            _context.BinhLuans.Remove(comment);
            await _context.SaveChangesAsync();
            TempData["CommentSuccess"] = "Bình luận đã được xóa!";

            return RedirectToAction("MyComments");
        }
    }
}
