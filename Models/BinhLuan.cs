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
        
        public int TrangThai { get; set; } = 0; // 0: Chờ duyệt, 1: Đã duyệt, 2: Từ chối
        
        public int? NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public int BaiVietId { get; set; }
        public BaiViet BaiViet { get; set; } = null!;
        
        // Reply functionality
        public int? ParentId { get; set; }
        public BinhLuan? Parent { get; set; }
        public ICollection<BinhLuan> Replies { get; set; } = new List<BinhLuan>();
    }
}
