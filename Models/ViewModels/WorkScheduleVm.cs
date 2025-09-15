using System;
using System.Collections.Generic;
using WEB_CV.Models;

namespace WEB_CV.Models.ViewModels
{
    public class WorkScheduleVm
    {
        public DateTime WeekStart { get; set; }      // thứ 2
        public DateTime WeekEnd { get; set; }        // chủ nhật
        public string? Keyword { get; set; }
        public string? Org { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Contact { get; set; }

        public Dictionary<DateTime, List<WorkScheduleEvent>> EventsByDay { get; set; } = new();
    }
}
