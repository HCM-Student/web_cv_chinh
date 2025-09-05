using System;

namespace WEB_CV.Models
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }

    public class Conversation
    {
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public int UnreadForUserA { get; set; }
        public int UnreadForUserB { get; set; }
        public DateTime LastMessageAtUtc { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
    }
}


