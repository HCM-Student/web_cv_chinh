using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models.Account
{
    public class RegisterVM
    {
        [Required, MaxLength(100)]
        public string HoTen { get; set; } = "";

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required, Compare(nameof(MatKhau)), DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = "";
    }
}
