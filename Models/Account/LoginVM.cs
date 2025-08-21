using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models.Account
{
    public class LoginVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        public bool GhiNho { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }
}
