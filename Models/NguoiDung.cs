namespace WEB_CV.Models
{
    public class NguoiDung
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;

        public ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
    }
}
