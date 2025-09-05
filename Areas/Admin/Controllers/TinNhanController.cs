using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Services;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TinNhanController : Controller
    {
        private readonly IMessagingService _messages;
        private readonly NewsDbContext _db;

        public TinNhanController(IMessagingService messages, NewsDbContext db)
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
    }
}


