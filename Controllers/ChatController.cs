using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Controllers;

[Authorize(Roles = "Admin,Staff")]   // chỉ Admin/Staff truy cập Chat
public class ChatController : Controller
{
    private readonly NewsDbContext _db;
    public ChatController(NewsDbContext db){ _db=db; }

    public async Task<IActionResult> Index(string room = "general")
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";
        
        ViewBag.MeId = userId;
        ViewBag.MeName = userName;
        ViewBag.Room = room;

        // Xử lý room DM
        string roomDisplayName = room;
        if (room.StartsWith("dm:"))
        {
            var parts = room.Replace("dm:", "").Split(":");
            var otherUserId = parts[0] == userId ? parts[1] : parts[0];
            var otherUser = await _db.NguoiDungs
                .Where(u => u.Id.ToString() == otherUserId)
                .Select(u => u.HoTen)
                .FirstOrDefaultAsync();
            roomDisplayName = otherUser ?? "Chat riêng tư";
        }

        ViewBag.RoomDisplayName = roomDisplayName;

        var msgs = await _db.ChatMessages
            .Where(x => x.Room == room)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var users = await _db.NguoiDungs
            .Where(u => u.VaiTro == "Admin" || u.VaiTro == "Staff")
            .OrderBy(u => u.HoTen)
            .Select(u => new { Id = u.Id.ToString(), UserName = u.HoTen })
            .ToListAsync();

        ViewBag.Users = users;
        return View(msgs);
    }
}
