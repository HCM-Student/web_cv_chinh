using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class LienHe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chủ đề")]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; } = DateTime.Now;
        
        public bool DaXuLy { get; set; } = false;
        
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chưa đọc";
        
        public string? GhiChu { get; set; }
    }
}
