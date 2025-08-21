namespace WEB_CV.Models
{
    public class ChuyenMuc
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;

        public ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
    }
}
