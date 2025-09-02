using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class ChuyenMuc
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Slug { get; set; }      // <— THÊM

        [MaxLength(1000)]
        public string? MoTa { get; set; }      // <— THÊM

        public ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
    }
}
