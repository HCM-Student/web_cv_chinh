using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class BinhLuan
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận")]
        [StringLength(1000, ErrorMessage = "Nội dung bình luận không được quá 1000 ký tự")]
        public string NoiDung { get; set; } = null!;
        
        public DateTime Ngay { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? HoTen { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        public bool DaDuyet { get; set; } = false;
        
        public int? NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public int BaiVietId { get; set; }
        public BaiViet BaiViet { get; set; } = null!;
    }
}
