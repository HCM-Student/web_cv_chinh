using System;
using System.Collections.Generic;

namespace WEB_CV.Models.ViewModels
{
    public class TinTucFilterVM
    {
        // Query
        public string? Q { get; set; }
        public int? ChuyenMucId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Sort { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;

        // Data for filters
        public List<ChuyenMuc> ChuyenMucs { get; set; } = new();

        // Results
        public List<BaiViet> Items { get; set; } = new();
        public int Total { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Math.Max(0, Total) / Math.Max(1, PageSize));

        // Sidebar
        public List<BaiViet> Latest5 { get; set; } = new();   // Bài viết mới
        public List<BaiViet> Recruit5 { get; set; } = new();  // Tuyển dụng mới
    }
}
