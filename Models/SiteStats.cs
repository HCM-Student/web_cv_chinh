using System;

namespace WEB_CV.Models
{
    public class SiteStats
    {
        public int Id { get; set; }            // IDENTITY column - không set giá trị mặc định
        public long TotalVisits { get; set; }   // tổng lượt truy cập web (unique theo cookie/ngày)
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
