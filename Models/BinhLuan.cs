namespace WEB_CV.Models
{
    public class BinhLuan
    {
        public int Id { get; set; }
        public string NoiDung { get; set; } = null!;
        public DateTime Ngay { get; set; } = DateTime.UtcNow;

        public int BaiVietId { get; set; }
        public BaiViet BaiViet { get; set; } = null!;
    }
}
