using System;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models;

public enum MessageStatus
{
    Sent = 0,       // đã lưu vào DB nhưng người nhận chưa online (chưa deliver)
    Delivered = 1,  // người nhận đang online (đã deliver tới client)
    Seen = 2        // người nhận đã mở cuộc trò chuyện
}

public class ChatMessage
{
    public int Id { get; set; }

    // "general" hoặc "dm:{a:b}" (a<b theo ordinal)
    [MaxLength(80)]
    public string Room { get; set; } = "general";

    [Required] public string SenderId { get; set; } = "";
    [Required, MaxLength(120)] public string SenderName { get; set; } = "";

    // Chỉ dùng cho DM 1-1
    [MaxLength(64)] public string? RecipientId { get; set; }

    [MaxLength(3000)]
    public string? Content { get; set; } = "";

    // Ảnh đính kèm (đơn giản 1 ảnh); có thể mở rộng bảng ChatAttachment nếu cần đa ảnh
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? SeenAt { get; set; }
}
