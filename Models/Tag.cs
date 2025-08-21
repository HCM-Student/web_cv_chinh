namespace WEB_CV.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public ICollection<BaiVietTag> BaiVietTags { get; set; } = new List<BaiVietTag>();
    }
}
