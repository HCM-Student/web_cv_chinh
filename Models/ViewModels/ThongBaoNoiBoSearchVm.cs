using System;
using System.Collections.Generic;
using WEB_CV.Models;

namespace WEB_CV.Models.ViewModels
{
    public class ThongBaoNoiBoSearchVm
    {
        // Search parameters
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
        public bool? IsPinned { get; set; }
        public bool? IsActive { get; set; } = true;

        // Results
        public List<InternalNotice> Notices { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Math.Max(1, TotalCount) / Math.Max(1, PageSize));
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
