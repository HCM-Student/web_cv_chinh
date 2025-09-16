using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Hubs;

[Authorize(Roles = "Admin,Staff")]
public class ChatHub : Hub
{
    private readonly NewsDbContext _db;
    public ChatHub(NewsDbContext db) => _db = db;

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "general");
        await base.OnConnectedAsync();
    }

    public Task JoinRoom(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);
    public Task LeaveRoom(string room) => Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

    public async Task SendToRoom(string room, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
        var msg = new ChatMessage { Room = room, SenderId = userId, SenderName = name, Content = message };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Group(room).SendAsync("message", new {
            id = msg.Id, room, senderId = userId, senderName = name,
            content = msg.Content, createdAt = msg.CreatedAt
        });
    }

    public async Task SendDirect(string toUserId, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
        var key = GetDmKey(userId, toUserId);
        var room = $"dm:{key}";

        var msg = new ChatMessage { Room = room, SenderId = userId, SenderName = name, RecipientId = toUserId, Content = message };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Users(new[] { userId, toUserId }).SendAsync("dm", new {
            id = msg.Id, room, senderId = userId, senderName = name, toUserId,
            content = msg.Content, createdAt = msg.CreatedAt
        });
    }

    private static string GetDmKey(string a, string b)
        => string.CompareOrdinal(a, b) < 0 ? $"{a}:{b}" : $"{b}:{a}";
}
