using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WEB_CV.Models
{
    public class SEOAnalysis
    {
        public int Id { get; set; }
        
        [Required]
        public int BaiVietId { get; set; }
        
        [ValidateNever]
        public BaiViet? BaiViet { get; set; }
        
        // Điểm tổng thể SEO (0-100)
        public int TongDiem { get; set; }
        
        // Các điểm chi tiết
        public int DiemTieuDe { get; set; }           // 0-100
        public int DiemTomTat { get; set; }           // 0-100
        public int DiemNoiDung { get; set; }          // 0-100
        public int DiemTuKhoa { get; set; }           // 0-100
        public int DiemCauTruc { get; set; }          // 0-100
        public int DiemHinhAnh { get; set; }          // 0-100
        public int DiemLienKet { get; set; }          // 0-100
        public int DiemDoDai { get; set; }            // 0-100
        
        // Thông tin chi tiết
        public string? TieuDePhanTich { get; set; }   // JSON chứa thông tin phân tích tiêu đề
        public string? TomTatPhanTich { get; set; }   // JSON chứa thông tin phân tích tóm tắt
        public string? NoiDungPhanTich { get; set; }  // JSON chứa thông tin phân tích nội dung
        public string? TuKhoaPhanTich { get; set; }   // JSON chứa thông tin phân tích từ khóa
        public string? CauTrucPhanTich { get; set; }  // JSON chứa thông tin phân tích cấu trúc
        public string? HinhAnhPhanTich { get; set; }  // JSON chứa thông tin phân tích hình ảnh
        public string? LienKetPhanTich { get; set; }  // JSON chứa thông tin phân tích liên kết
        public string? DoDaiPhanTich { get; set; }    // JSON chứa thông tin phân tích độ dài
        
        // Gợi ý cải thiện
        public string? GoiYCaiThien { get; set; }     // JSON chứa danh sách gợi ý
        
        // Thời gian phân tích
        public DateTime NgayPhanTich { get; set; } = DateTime.UtcNow;
        
        // Trạng thái
        public bool DaXuLy { get; set; } = false;
    }
    
    // ViewModel để hiển thị kết quả phân tích
    public class SEOAnalysisVM
    {
        public int BaiVietId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public int TongDiem { get; set; }
        public string MauSac { get; set; } = string.Empty; // red, yellow, green
        public string TrangThai { get; set; } = string.Empty; // Kém, Trung bình, Tốt, Xuất sắc
        
        // Chi tiết các điểm
        public SEOChiTietDiem TieuDeChiTiet { get; set; } = new();
        public SEOChiTietDiem TomTatChiTiet { get; set; } = new();
        public SEOChiTietDiem NoiDungChiTiet { get; set; } = new();
        public SEOChiTietDiem TuKhoaChiTiet { get; set; } = new();
        public SEOChiTietDiem CauTrucChiTiet { get; set; } = new();
        public SEOChiTietDiem HinhAnhChiTiet { get; set; } = new();
        public SEOChiTietDiem LienKetChiTiet { get; set; } = new();
        public SEOChiTietDiem DoDaiChiTiet { get; set; } = new();
        
        // Gợi ý cải thiện
        public List<string> GoiY { get; set; } = new();
    }
    
    public class SEOChiTietDiem
    {
        public int Diem { get; set; }
        public string MauSac { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public List<string> GhiChu { get; set; } = new();
        public bool ThanhCong { get; set; }
    }
}
