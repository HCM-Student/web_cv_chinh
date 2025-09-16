using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Hubs;

[Authorize(Roles = "Admin,Staff")]
public class ChatHub : Hub
{
    private readonly NewsDbContext _db;
    public ChatHub(NewsDbContext db) => _db = db;

    // Quản lý online presence
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    private static bool IsOnline(string userId)
        => _userConnections.TryGetValue(userId, out var set) && set.Count > 0;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var set = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
            lock (set) set.Add(Context.ConnectionId);

            // Khi user vừa online, auto "Delivered" các tin nhắn gửi cho họ đang ở trạng thái Sent
            var now = DateTime.UtcNow;
            var needDeliver = await _db.ChatMessages
                .Where(m => m.RecipientId == userId && m.Status == MessageStatus.Sent)
                .Select(m => new { m.Id, m.SenderId })
                .ToListAsync();

            if (needDeliver.Count > 0)
            {
                // Update DB
                await _db.ChatMessages
                    .Where(m => m.RecipientId == userId && m.Status == MessageStatus.Sent)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.Status, MessageStatus.Delivered)
                        .SetProperty(m => m.DeliveredAt, now));

                // Báo về người gửi (và cả người nhận) để cập nhật UI
                foreach (var item in needDeliver)
                {
                    await Clients.Users(new[] { item.SenderId, userId }).SendAsync("statusChanged", new
                    {
                        id = item.Id,
                        status = "Delivered",
                        deliveredAt = now
                    });
                }
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId) && _userConnections.TryGetValue(userId, out var set))
        {
            lock (set) set.Remove(Context.ConnectionId);
            if (set.Count == 0) _userConnections.TryRemove(userId, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Join room public hay room DM
    public Task JoinRoom(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);

    // Chat phòng chung (không áp dụng trạng thái Delivered/Seen cho từng người)
    public async Task SendToRoom(string room, string? message, string? imageUrl = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";

        var msg = new ChatMessage
        {
            Room = room,
            SenderId = userId,
            SenderName = name,
            Content = message,
            ImageUrl = imageUrl
        };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Group(room).SendAsync("message", new
        {
            id = msg.Id,
            room,
            senderId = userId,
            senderName = name,
            content = msg.Content,
            imageUrl = msg.ImageUrl,
            createdAt = msg.CreatedAt,
            status = "Sent"
        });
    }

    // Chat 1-1: status Sent/Delivered/Seen
    public async Task SendDirect(string toUserId, string? message, string? imageUrl = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";

        var key = GetDmKey(userId, toUserId);
        var room = $"dm:{key}";

        var msg = new ChatMessage
        {
            Room = room,
            SenderId = userId,
            SenderName = name,
            RecipientId = toUserId,
            Content = message,
            ImageUrl = imageUrl,
            Status = IsOnline(toUserId) ? MessageStatus.Delivered : MessageStatus.Sent,
            DeliveredAt = IsOnline(toUserId) ? DateTime.UtcNow : null
        };

        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Users(new[] { userId, toUserId }).SendAsync("dm", new
        {
            id = msg.Id,
            room,
            senderId = userId,
            senderName = name,
            toUserId,
            content = msg.Content,
            imageUrl = msg.ImageUrl,
            createdAt = msg.CreatedAt,
            status = msg.Status.ToString(),
            deliveredAt = msg.DeliveredAt
        });
    }

    // Khi người nhận mở khung DM với người gửi -> mark seen
    public async Task MarkSeen(string otherUserId)
    {
        var me = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(me) || string.IsNullOrEmpty(otherUserId)) return;

        var now = DateTime.UtcNow;

        // Lấy tất cả message do otherUser gửi cho me, chưa Seen
        var toUpdate = await _db.ChatMessages
            .Where(m => m.SenderId == otherUserId
                        && m.RecipientId == me
                        && m.Status != MessageStatus.Seen)
            .Select(m => m.Id)
            .ToListAsync();

        if (toUpdate.Count == 0) return;

        await _db.ChatMessages
            .Where(m => toUpdate.Contains(m.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, MessageStatus.Seen)
                .SetProperty(m => m.SeenAt, now)
                .SetProperty(m => m.DeliveredAt, m => m.DeliveredAt ?? now)); // nếu chưa delivered thì coi như delivered luôn

        // broadcast về 2 phía
        foreach (var id in toUpdate)
        {
            await Clients.Users(new[] { me, otherUserId }).SendAsync("statusChanged", new
            {
                id,
                status = "Seen",
                seenAt = now
            });
        }
    }

    private static string GetDmKey(string a, string b)
        => string.CompareOrdinal(a, b) < 0 ? $"{a}:{b}" : $"{b}:{a}";
}
