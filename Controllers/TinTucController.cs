using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

public class TinTucController : Controller
{
    private readonly NewsDbContext _db;
    public TinTucController(NewsDbContext db) => _db = db;

    // /TinTuc
    public async Task<IActionResult> Index(string? q, int? cm, int page = 1, int pageSize = 9)
    {
        var query = _db.BaiViets
            .Include(x => x.ChuyenMuc)
            .OrderByDescending(x => x.NgayDang)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.TieuDe.Contains(q) || (x.TomTat ?? "").Contains(q));
        if (cm.HasValue)
            query = query.Where(x => x.ChuyenMucId == cm.Value);

        var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View(posts);
    }

    // /TinTuc/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var bv = await _db.BaiViets
            .Include(x => x.ChuyenMuc)
            .Include(x => x.TacGia)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (bv == null) return NotFound();
        
        // Tăng lượt xem
        bv.LuotXem++;
        await _db.SaveChangesAsync();
        
        // Lấy bài viết liên quan
        var relatedPosts = await _db.BaiViets
            .Include(x => x.ChuyenMuc)
            .Where(x => x.Id != id && x.ChuyenMucId == bv.ChuyenMucId)
            .OrderByDescending(x => x.NgayDang)
            .Take(4)
            .ToListAsync();
        
        ViewBag.RelatedPosts = relatedPosts;
        
        return View(bv);
    }
}
