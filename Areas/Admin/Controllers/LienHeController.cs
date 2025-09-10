using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LienHeController : Controller
    {
        private readonly NewsDbContext _context;

        public LienHeController(NewsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lienHes = await _context.LienHes
                .AsNoTracking()
                .OrderByDescending(lh => lh.NgayGui)
                .ToListAsync();

            return View(lienHes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lienHe = await _context.LienHes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lienHe == null) return NotFound();
            return View(lienHe);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lienHe = await _context.LienHes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lienHe == null) return NotFound();
            return View(lienHe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe != null)
            {
                _context.LienHes.Remove(lienHe);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
