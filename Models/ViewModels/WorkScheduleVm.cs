using System;
using System.Collections.Generic;
using WEB_CV.Models;

namespace WEB_CV.Models.ViewModels
{
    public class WorkScheduleVm
    {
        public DateTime WeekStart { get; set; }      // thứ 2
        public DateTime WeekEnd { get; set; }        // chủ nhật
        public int WeekNo { get; set; }              // số tuần
        public string? Keyword { get; set; }
        public string? Leader { get; set; }          // lãnh đạo
        public List<string> LeaderOptions { get; set; } = new(); // danh sách lãnh đạo

        public Dictionary<DateTime, List<WorkScheduleEvent>> EventsByDay { get; set; } = new();
    }
}
