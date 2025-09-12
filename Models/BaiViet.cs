using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WEB_CV.Models
{
    public class BaiViet
    {
        public int Id { get; set; }

        // ===== Tiếng Việt =====
        [Required, StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        [StringLength(500)]
        public string? TomTat { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        // ===== Tiếng Anh (tùy chọn) =====
        [StringLength(255)]
        public string? TieuDeEn { get; set; }

        [StringLength(500)]
        public string? TomTatEn { get; set; }

        /// <summary>
        /// Nội dung tiếng Anh; có thể chứa HTML giống NộiDung
        /// </summary>
        public string? NoiDungEn { get; set; }

        // ===== Danh mục / Tác giả =====
        [Required]
        public int ChuyenMucId { get; set; }

        [ValidateNever]
        public ChuyenMuc? ChuyenMuc { get; set; }

        [Required]
        public int TacGiaId { get; set; }

        [ValidateNever]
        public NguoiDung? TacGia { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.UtcNow;
        
        public int LuotXem { get; set; } = 0;

        // ===== Scheduled Publishing =====
        /// <summary>
        /// Thời gian đăng bài đã lên lịch (nếu có)
        /// </summary>
        public DateTime? NgayDangDuKien { get; set; }

        /// <summary>
        /// Trạng thái bài viết: 0=Draft, 1=Published, 2=Scheduled
        /// </summary>
        public int TrangThai { get; set; } = 1; // 0: Draft, 1: Published, 2: Scheduled

        // ===== Ảnh đại diện =====
        [StringLength(300)]
        public string? AnhTieuDe { get; set; }   // ví dụ: "media/posts/cover/xxx.jpg" hoặc URL tuyệt đối

        [StringLength(200)]
        public string? AnhTieuDeAlt { get; set; } // mô tả ảnh (SEO/Accessibility)

        // ===== Video =====
        [StringLength(300)]
        public string? VideoFile { get; set; }   // đường dẫn file video (ví dụ: "media/posts/videos/xxx.mp4")

        [StringLength(200)]
        public string? VideoAlt { get; set; }    // mô tả video (SEO/Accessibility)

        [StringLength(500)]
        public string? VideoUrl { get; set; }    // URL video từ YouTube, Vimeo, etc.

        [StringLength(100)]
        public string? VideoType { get; set; }   // "file", "youtube", "vimeo", etc.

        // ===== Điều hướng/ liên kết =====
        [ValidateNever]
        public ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

        [ValidateNever]
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new List<BaiVietTag>();
    }
}
