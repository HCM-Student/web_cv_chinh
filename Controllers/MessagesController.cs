using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Services;

namespace web_cv.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessagingService _messages;
        private readonly NewsDbContext _db;

        public MessagesController(IMessagingService messages, NewsDbContext db)
        {
            _messages = messages;
            _db = db;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Index()
        {
            var uid = CurrentUserId;
            var conv = await _messages.GetConversationsAsync(uid);
            return View(conv);
        }

        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            // Find an admin user to chat with
            var admin = await _db.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(x => x.VaiTro == "Admin");
            if (admin == null) return NotFound("Không tìm thấy quản trị viên");
            return RedirectToAction(nameof(Chat), new { with = admin.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Chat(int with)
        {
            var uid = CurrentUserId;
            var other = await _db.NguoiDungs.FirstOrDefaultAsync(x => x.Id == with);
            if (other == null) return NotFound();

            ViewBag.OtherUser = other;
            ViewBag.CurrentUserId = uid;
            await _messages.MarkAsReadAsync(uid, with);
            var msgs = await _messages.GetMessagesAsync(uid, with, 200);
            return View(msgs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int toUserId, string content)
        {
            var uid = CurrentUserId;
            await _messages.SendMessageAsync(uid, toUserId, content);
            return RedirectToAction(nameof(Chat), new { with = toUserId });
        }

        [HttpGet]
        [Route("api/admin/user-id")]
        public async Task<IActionResult> GetAdminUserId()
        {
            var admin = await _db.NguoiDungs.FirstOrDefaultAsync(x => x.VaiTro == "Admin");
            return Json(new { userId = admin?.Id ?? 1 });
        }
    }
}


