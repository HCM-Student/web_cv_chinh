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

        // ===== Ảnh đại diện =====
        [StringLength(300)]
        public string? AnhTieuDe { get; set; }   // ví dụ: "media/posts/cover/xxx.jpg" hoặc URL tuyệt đối

        [StringLength(200)]
        public string? AnhTieuDeAlt { get; set; } // mô tả ảnh (SEO/Accessibility)

        // ===== Điều hướng/ liên kết =====
        [ValidateNever]
        public ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

        [ValidateNever]
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new List<BaiVietTag>();
    }
}
