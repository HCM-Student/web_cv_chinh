using System;
using System.Collections.Generic;
using WEB_CV.Models;

namespace WEB_CV.Areas.Admin.ViewModels
{
    public class InternalNoticeFilterVm
    {
        public string? q { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public int ps { get; set; } = 10;  // page size
        public int page { get; set; } = 1; // page number (1-based)
        public bool? onlyActive { get; set; }

        // Kết quả
        public List<InternalNotice> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pages => (int)Math.Ceiling((double)Math.Max(1, Total) / Math.Max(1, ps));
    }
}
