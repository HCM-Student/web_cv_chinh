using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class BaiViet
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string TieuDe { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TomTat { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime NgayDang { get; set; } = DateTime.UtcNow;

        // FK -> Chuyên mục
        [Required]
        public int ChuyenMucId { get; set; }
        public ChuyenMuc ChuyenMuc { get; set; } = null!;

        // FK -> Tác giả (NguoiDung)
        [Required]
        public int TacGiaId { get; set; }
        public NguoiDung TacGia { get; set; } = null!;

        // Điều hướng
        public ICollection<BinhLuan> BinhLuans { get; set; } = new HashSet<BinhLuan>();
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new HashSet<BaiVietTag>();
    }
}
