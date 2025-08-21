namespace WEB_CV.Models
{
    public class BaiViet
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = null!;
        public string? TomTat { get; set; }
        public string NoiDung { get; set; } = null!;
        public DateTime NgayDang { get; set; } = DateTime.UtcNow;

        public int ChuyenMucId { get; set; }
        public ChuyenMuc ChuyenMuc { get; set; } = null!;

        public int TacGiaId { get; set; }
        public NguoiDung TacGia { get; set; } = null!;

        public ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new List<BaiVietTag>();
    }
}
