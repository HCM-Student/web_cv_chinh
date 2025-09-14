using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_CV.Services;

namespace WEB_CV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineUserController : ControllerBase
    {
        private readonly IOnlineUserService _onlineUserService;

        public OnlineUserController(IOnlineUserService onlineUserService)
        {
            _onlineUserService = onlineUserService;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetOnlineUsersCount([FromQuery] int minutes = 10)
        {
            try
            {
                var count = await _onlineUserService.GetOnlineUsersCountAsync(minutes);
                return Ok(new { count, minutes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy số người dùng online", details = ex.Message });
            }
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetOnlineUsers([FromQuery] int minutes = 10)
        {
            try
            {
                var users = await _onlineUserService.GetOnlineUsersAsync(minutes);
                var result = users.Select(u => new
                {
                    id = u.Id,
                    hoTen = u.HoTen ?? "Khách",
                    email = u.Email,
                    lastSeen = u.LastSeen,
                    ipAddress = u.IpAddress
                }).ToList();

                return Ok(new { users = result, count = result.Count, minutes });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy danh sách người dùng online" });
            }
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackUser([FromBody] TrackUserRequest request)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                await _onlineUserService.AddOrUpdateUserAsync(
                    sessionId, 
                    request.Email, 
                    request.HoTen, 
                    ipAddress, 
                    userAgent
                );

                return Ok(new { 
                    success = true, 
                    sessionId = sessionId.Substring(0, Math.Min(8, sessionId.Length)) + "...",
                    ipAddress,
                    userAgent = userAgent.Substring(0, Math.Min(50, userAgent.Length)) + "..."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi theo dõi người dùng", details = ex.Message });
            }
        }


        [HttpPost("cleanup")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupInactiveUsers([FromQuery] int minutes = 10)
        {
            try
            {
                await _onlineUserService.CleanupInactiveUsersAsync(minutes);
                return Ok(new { success = true, message = "Đã dọn dẹp người dùng không hoạt động" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Lỗi khi dọn dẹp người dùng" });
            }
        }
    }

    public class TrackUserRequest
    {
        public string? Email { get; set; }
        public string? HoTen { get; set; }
    }
}
