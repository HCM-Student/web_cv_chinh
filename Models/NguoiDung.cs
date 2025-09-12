using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;   // <== THÊM DÒNG NÀY

namespace WEB_CV.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string HoTen { get; set; } = "";

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string MatKhauHash { get; set; } = "";

        [Required, MaxLength(20)]
        public string VaiTro { get; set; } = "User";

        public bool KichHoat { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public string? Avatar { get; set; }

        // ===== THÊM MỚI: danh sách bài viết do user này làm tác giả =====
        public virtual ICollection<BaiViet> BaiViets { get; set; } = new HashSet<BaiViet>();
    }
}
