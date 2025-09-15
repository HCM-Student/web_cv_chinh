using System;
using System.ComponentModel.DataAnnotations;

namespace WEB_CV.Models
{
    public class WorkScheduleEvent
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }          // ngày diễn ra (chỉ dùng phần Date)

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }    // giờ bắt đầu

        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }      // giờ kết thúc

        [Required, MaxLength(300)]
        public string Title { get; set; } = "";     // Nội dung

        [MaxLength(200)]
        public string Location { get; set; } = "";  // Địa điểm

        [MaxLength(200)]
        public string Organization { get; set; } = ""; // Tên tổ chức

        [MaxLength(300)]
        public string Participants { get; set; } = ""; // Thành phần tham gia

        [MaxLength(300)]
        public string Preparation { get; set; } = ""; // Thành phần chuẩn bị

        [MaxLength(200)]
        public string Contact { get; set; } = "";      // Liên hệ (tên/người phụ trách)

        [MaxLength(50)]
        public string Phone { get; set; } = "";

        [MaxLength(120), EmailAddress]
        public string Email { get; set; } = "";
    }
}
