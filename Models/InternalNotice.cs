using System;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class InternalNotice
    {
        public int Id { get; set; }

        [Required, MaxLength(260)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required]
        public string Body { get; set; } = "";

        /// <summary>Thời điểm bắt đầu hiển thị</summary>
        public DateTime PublishAt { get; set; } = DateTime.UtcNow;

        /// <summary>Hết hạn (nếu có)</summary>
        public DateTime? ExpireAt { get; set; }

        public bool IsActive { get; set; } = true;   // Ẩn/Hiện
        public bool IsPinned { get; set; } = false;  // Ghim

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // optional: lưu người tạo
        [MaxLength(64)] public string? CreatedById { get; set; }
        [MaxLength(120)] public string? CreatedByName { get; set; }
    }
}
