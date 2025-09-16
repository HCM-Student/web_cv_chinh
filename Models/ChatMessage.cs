using System;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Room { get; set; } = "general";   // "general" hoặc "dm:{a:b}"

    [Required] public string SenderId { get; set; } = "";
    [Required, MaxLength(120)] public string SenderName { get; set; } = "";
    [MaxLength(64)] public string? RecipientId { get; set; } // cho 1–1

    [Required, MaxLength(3000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
