using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WEB_CV.Models
{
    public class BaiViet
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        [StringLength(500)]
        public string? TomTat { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        [Required]
        public int ChuyenMucId { get; set; }

        [ValidateNever]
        public ChuyenMuc? ChuyenMuc { get; set; }

        [Required]
        public int TacGiaId { get; set; }

        [ValidateNever]
        public NguoiDung? TacGia { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.UtcNow;

        // ====== BỔ SUNG để khớp với NewsDbContext ======
        [ValidateNever]
        public ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

        [ValidateNever]
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new List<BaiVietTag>();

        [StringLength(300)]
        public string? AnhTieuDe { get; set; }   // lưu URL: "/media/posts/cover/xxx.jpg"

        [StringLength(200)]
        public string? AnhTieuDeAlt { get; set; } // mô tả ảnh (SEO/Accessibility)
    }
}
