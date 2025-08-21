namespace WEB_CV.Models
{
    public class BaiVietTag
    {
        public int BaiVietId { get; set; }
        public BaiViet BaiViet { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
