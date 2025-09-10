using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class LienHe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [StringLength(200)]
        public string? TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}
