using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class OnlineUser
    {
        [Key]
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string SessionId { get; set; } = "";
        
        [MaxLength(150)]
        public string? Email { get; set; }
        
        [MaxLength(100)]
        public string? HoTen { get; set; }
        
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        
        [MaxLength(200)]
        public string? UserAgent { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
