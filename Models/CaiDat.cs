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
        public string TenWebsite { get; set; } = string.Empty;
        public string Slogan { get; set; } = string.Empty;
        public string MoTaWebsite { get; set; } = string.Empty;
        public string EmailLienHe { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string MuiGio { get; set; } = "UTC+7";
        public int SoBaiVietMoiTrang { get; set; } = 10;
        public string DinhDangNgay { get; set; } = "dd/MM/yyyy";
        public string NgonNguMacDinh { get; set; } = "vi";
    }

    public class CaiDatBaoMatVM
    {
        public int DoDaiMatKhauToiThieu { get; set; } = 8;
        public int ThoiGianHetHanMatKhau { get; set; } = 90;
        public bool YeuCauChuHoa { get; set; } = true;
        public bool YeuCauSo { get; set; } = true;
        public bool YeuCauKyTuDacBiet { get; set; } = true;
        public int SoLanDangNhapSaiToiDa { get; set; } = 5;
        public int ThoiGianKhoaTaiKhoan { get; set; } = 30;
        public bool KichHoatXacThuc2YeuTo { get; set; } = false;
        public bool GuiThongBaoDangNhapMoi { get; set; } = true;
    }

    public class CaiDatEmailVM
    {
        public string MayKhuSMTP { get; set; } = string.Empty;
        public int CongSMTP { get; set; } = 587;
        public string BaoMatSMTP { get; set; } = "TLS";
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string TenNguoiGui { get; set; } = string.Empty;
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
