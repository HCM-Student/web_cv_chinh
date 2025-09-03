using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_CV.Models
{
    [Table("CaiDats")]
    public class CaiDat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "string"; // string, int, bool, json

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? NgCapNhatBoi { get; set; }
    }

    // ViewModels cho các form cài đặt
    public class CaiDatChungVM
    {
        [Required(ErrorMessage = "Tên website không được để trống")]
        [StringLength(200, ErrorMessage = "Tên website không được vượt quá 200 ký tự")]
        public string TenWebsite { get; set; } = string.Empty;
        
        [StringLength(300, ErrorMessage = "Slogan không được vượt quá 300 ký tự")]
        public string Slogan { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả website không được vượt quá 1000 ký tự")]
        public string MoTaWebsite { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Email liên hệ không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string EmailLienHe { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string SoDienThoai { get; set; } = string.Empty;
        
        public string MuiGio { get; set; } = "UTC+7";
        
        [Range(1, 100, ErrorMessage = "Số bài viết mỗi trang phải từ 1 đến 100")]
        public int SoBaiVietMoiTrang { get; set; } = 10;
        
        public string DinhDangNgay { get; set; } = "dd/MM/yyyy";
        
        public string NgonNguMacDinh { get; set; } = "vi";
    }

    public class CaiDatBaoMatVM
    {
        [Range(6, 20, ErrorMessage = "Độ dài mật khẩu tối thiểu phải từ 6 đến 20")]
        public int DoDaiMatKhauToiThieu { get; set; } = 8;
        
        [Range(30, 365, ErrorMessage = "Thời gian hết hạn mật khẩu phải từ 30 đến 365 ngày")]
        public int ThoiGianHetHanMatKhau { get; set; } = 90;
        
        public bool YeuCauChuHoa { get; set; } = true;
        public bool YeuCauSo { get; set; } = true;
        public bool YeuCauKyTuDacBiet { get; set; } = true;
        
        [Range(3, 10, ErrorMessage = "Số lần đăng nhập sai tối đa phải từ 3 đến 10")]
        public int SoLanDangNhapSaiToiDa { get; set; } = 5;
        
        [Range(5, 1440, ErrorMessage = "Thời gian khóa tài khoản phải từ 5 đến 1440 phút")]
        public int ThoiGianKhoaTaiKhoan { get; set; } = 30;
        
        public bool KichHoatXacThuc2YeuTo { get; set; } = false;
        public bool GuiThongBaoDangNhapMoi { get; set; } = true;
    }

    public class CaiDatEmailVM
    {
        [Required(ErrorMessage = "Máy chủ SMTP không được để trống")]
        [StringLength(255, ErrorMessage = "Máy chủ SMTP không được vượt quá 255 ký tự")]
        public string MayKhuSMTP { get; set; } = string.Empty;
        
        [Range(1, 65535, ErrorMessage = "Cổng SMTP phải từ 1 đến 65535")]
        public int CongSMTP { get; set; } = 587;
        
        public string BaoMatSMTP { get; set; } = "TLS";
        
        [EmailAddress(ErrorMessage = "Tên đăng nhập phải là email hợp lệ")]
        [StringLength(255, ErrorMessage = "Tên đăng nhập không được vượt quá 255 ký tự")]
        public string TenDangNhap { get; set; } = string.Empty;
        
        [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự")]
        public string MatKhau { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Tên người gửi không được vượt quá 200 ký tự")]
        public string TenNguoiGui { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Email người gửi không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email người gửi không được vượt quá 255 ký tự")]
        public string EmailNguoiGui { get; set; } = string.Empty;
        
        public bool GuiEmailNguoiDungMoi { get; set; } = true;
        public bool ThongBaoBaiVietMoi { get; set; } = true;
        public bool ThongBaoPhanHoi { get; set; } = false;
    }

    public class CaiDatGiaoDienVM
    {
        public string ChuDeManSac { get; set; } = "blue";
        public string CustomCSS { get; set; } = string.Empty;
    }

    public class CaiDatBackupVM
    {
        public string TanSuatBackup { get; set; } = "weekly";
        public string GioThucHien { get; set; } = "02:00";
        public int SoLuongBackupGiuLai { get; set; } = 10;
        public string ThuMucLuuTru { get; set; } = "/backups/";
    }
}
