using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Controllers;

[Authorize(Roles = "Admin,TruongPhongPhatTrien,TruongPhongNhanSu,TruongPhongDuLieu,Staff")]
public class ChatController : Controller
{
    private readonly NewsDbContext _db;
    private readonly IWebHostEnvironment _env;
    public ChatController(NewsDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // room = "general" hoặc "dm:{a:b}"
    public async Task<IActionResult> Index(string room = "general")
    {
        var meId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var meName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        ViewBag.MeId = meId;
        ViewBag.MeName = meName;
        ViewBag.Room = room;

        // Danh sách user để mở DM - tất cả vai trò quan trọng
        var users = await _db.NguoiDungs
            .Where(u => u.VaiTro == "Admin" || u.VaiTro == "TruongPhongPhatTrien" || 
                       u.VaiTro == "TruongPhongNhanSu" || u.VaiTro == "TruongPhongDuLieu" || 
                       u.VaiTro == "Staff")
            .OrderBy(u => u.HoTen)
            .Select(u => new { Id = u.Id.ToString(), UserName = u.HoTen, Avatar = u.Avatar })
            .ToListAsync();
        ViewBag.Users = users;

        // Lấy 100 tin gần nhất của room với thông tin avatar
        var msgs = await _db.ChatMessages
            .Where(m => m.Room == room)
            .OrderByDescending(m => m.Id)
            .Take(100)
            .OrderBy(m => m.Id)
            .ToListAsync();

        // Tạo dictionary để lưu avatar của từng user
        var userIds = msgs.Select(m => m.SenderId).Distinct().ToList();
        var userAvatars = await _db.NguoiDungs
            .Where(u => userIds.Contains(u.Id.ToString()))
            .ToDictionaryAsync(u => u.Id.ToString(), u => u.Avatar);
        
        ViewBag.UserAvatars = userAvatars;

        return View(msgs);
    }

    // Upload ảnh đính kèm (chỉ Admin/Staff)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file");

        // Chỉ cho ảnh
        var okTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!okTypes.Contains(file.ContentType))
            return BadRequest("Chỉ cho phép upload ảnh");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var now = DateTime.UtcNow;
        var folder = Path.Combine(_env.WebRootPath, "uploads", "chat", now.ToString("yyyy"), now.ToString("MM"));
        Directory.CreateDirectory(folder);

        var fname = $"{Guid.NewGuid():N}{ext}";
        var full = Path.Combine(folder, fname);
        using (var fs = System.IO.File.Create(full))
        {
            await file.CopyToAsync(fs);
        }

        var url = $"/uploads/chat/{now:yyyy}/{now:MM}/{fname}";
        return Json(new { url });
    }
}
