using System.Collections.Generic;

namespace WEB_CV.Models
{
    public class ChiTietBaiVietVM
    {
        public BaiViet BaiViet { get; set; } = null!;
        public List<BaiViet> BaiVietLienQuan { get; set; } = new();
        public List<BaiViet> BaiVietMoiNhat { get; set; } = new();
    }
}
