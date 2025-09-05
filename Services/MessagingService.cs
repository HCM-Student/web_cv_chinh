using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly NewsDbContext _db;

        public MessagingService(NewsDbContext db)
        {
            _db = db;
        }

        public async Task<List<Conversation>> GetConversationsAsync(int currentUserId)
        {
            // Get all messages for current user
            var messages = await _db.Messages
                .Where(m => m.FromUserId == currentUserId || m.ToUserId == currentUserId)
                .ToListAsync();

            // Group by other user ID
            var groupedMessages = messages
                .GroupBy(m => m.FromUserId == currentUserId ? m.ToUserId : m.FromUserId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(x => x.SentAtUtc).First(),
                    UnreadCount = g.Count(x => x.ToUserId == currentUserId && !x.IsRead)
                })
                .OrderByDescending(x => x.LastMessage.SentAtUtc)
                .ToList();

            return groupedMessages.Select(x => new Conversation
            {
                UserAId = currentUserId,
                UserBId = x.OtherUserId,
                UnreadForUserA = x.UnreadCount,
                UnreadForUserB = 0,
                LastMessageAtUtc = x.LastMessage.SentAtUtc,
                LastMessagePreview = x.LastMessage.Content.Length > 120 ? x.LastMessage.Content.Substring(0, 120) + "…" : x.LastMessage.Content
            }).ToList();
        }

        public async Task<List<Message>> GetMessagesAsync(int userAId, int userBId, int take = 100, int skip = 0)
        {
            return await _db.Messages
                .Where(m => (m.FromUserId == userAId && m.ToUserId == userBId) || (m.FromUserId == userBId && m.ToUserId == userAId))
                .OrderBy(m => m.SentAtUtc)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Message> SendMessageAsync(int fromUserId, int toUserId, string content)
        {
            content = (content ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Nội dung không được rỗng", nameof(content));

            var msg = new Message
            {
                Id = Guid.NewGuid().ToString(),
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Content = content,
                SentAtUtc = DateTime.UtcNow,
                IsRead = false
            };
            
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();
            return msg;
        }

        public async Task MarkAsReadAsync(int readerUserId, int otherUserId)
        {
            var messages = await _db.Messages
                .Where(m => m.ToUserId == readerUserId && m.FromUserId == otherUserId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            if (messages.Any())
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}


