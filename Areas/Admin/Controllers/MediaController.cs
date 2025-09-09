using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WEB_CV.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MediaController : Controller
    {
        private readonly IWebHostEnvironment _env;

        // Định dạng cho phép
        private static readonly string[] _allowExt = new[]
        {
            ".png",".jpg",".jpeg",".webp",".gif",".svg",
            ".mp4",".webm",".mp3",".wav",
            ".pdf",".doc",".docx",".xls",".xlsx",".ppt",".pptx",".txt"
        };

        public MediaController(IWebHostEnvironment env) => _env = env;

        // /wwwroot/media
        private string RootPath => Path.Combine(_env.WebRootPath, "media");

        // Chuẩn hoá đường dẫn, chống path traversal
        private string SafePath(string? rel)
        {
            rel ??= "";
            rel = rel.Replace('\\', '/').TrimStart('/');
            var full = Path.GetFullPath(Path.Combine(RootPath, rel));
            if (!full.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid path");
            return full;
        }

        public IActionResult Index() => View();

        // GET: /Admin/Media/List?folder=xxx&q=keyword
        [HttpGet]
        public IActionResult List(string? folder = "", string? q = "")
        {
            var dir = SafePath(folder);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var provider = new FileExtensionContentTypeProvider();

            var dirs = Directory.GetDirectories(dir)
                .Select(d => new
                {
                    type = "folder",
                    name = Path.GetFileName(d),
                    path = Path.GetRelativePath(RootPath, d).Replace("\\", "/")
                });

            var files = Directory.GetFiles(dir)
                .Select(p =>
                {
                    var fi = new FileInfo(p);
                    provider.TryGetContentType(p, out var ct);
                    var rel = Path.GetRelativePath(RootPath, p).Replace("\\", "/");
                    return new
                    {
                        type = "file",
                        name = fi.Name,
                        path = rel,
                        url = "/" + ("media/" + rel).Replace("\\", "/"),
                        size = fi.Length,
                        modified = fi.LastWriteTimeUtc,
                        contentType = ct ?? "application/octet-stream"
                    };
                });

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim().ToLowerInvariant();
                dirs = dirs.Where(x => x.name.ToLower().Contains(kw));
                files = files.Where(x => x.name.ToLower().Contains(kw));
            }

            return Json(new
            {
                ok = true,
                folder = (folder ?? "").Replace("\\", "/"),
                items = dirs.Concat<object>(files)
            });
        }

        // POST: /Admin/Media/Upload  (FilePond: gửi 1 file/1 request)
        [HttpPost]
        [RequestSizeLimit(1024L * 1024 * 200)] // 200MB
        public async Task<IActionResult> Upload(string? folder = "")
        {
            var targetDir = SafePath(folder);
            Directory.CreateDirectory(targetDir);

            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
                return BadRequest("No file uploaded");

            foreach (var f in files)
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (!_allowExt.Contains(ext)) return BadRequest($"Không hỗ trợ định dạng: {ext}");
                if (f.Length <= 0) continue;

                var baseName = Path.GetFileNameWithoutExtension(f.FileName).Trim();
                var fileName = $"{baseName}{ext}";
                var dst = Path.Combine(targetDir, fileName);

                // tránh ghi đè
                int i = 1;
                while (System.IO.File.Exists(dst))
                {
                    fileName = $"{baseName}-{i++}{ext}";
                    dst = Path.Combine(targetDir, fileName);
                }

                await using var stream = System.IO.File.Create(dst);
                await f.CopyToAsync(stream);
            }

            // FilePond thích response text/plain
            return Content("ok", "text/plain");
        }

        // POST: /Admin/Media/Delete
        [HttpPost]
        public IActionResult Delete(string path)
        {
            var full = SafePath(path);
            if (System.IO.File.Exists(full))
            {
                System.IO.File.Delete(full);
                return Json(new { ok = true });
            }
            if (Directory.Exists(full))
            {
                Directory.Delete(full, recursive: true);
                return Json(new { ok = true });
            }
            return NotFound();
        }

        // POST: /Admin/Media/CreateFolder
        [HttpPost]
        public IActionResult CreateFolder(string name, string? parent = "")
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Tên trống");
            var full = SafePath(Path.Combine(parent ?? "", name));
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            return Json(new { ok = true });
        }

        // POST: /Admin/Media/Rename
        [HttpPost]
        public IActionResult Rename(string path, string newName)
        {
            newName = newName.Trim();
            if (string.IsNullOrWhiteSpace(newName)) return BadRequest("Tên trống");

            var src = SafePath(path);
            var parent = Path.GetDirectoryName(src)!;
            var dst = SafePath(Path.Combine(Path.GetRelativePath(RootPath, parent), newName));

            if (System.IO.File.Exists(src))
            {
                if (System.IO.File.Exists(dst)) return Conflict("Tên đã tồn tại");
                System.IO.File.Move(src, dst);
                return Json(new { ok = true });
            }
            if (Directory.Exists(src))
            {
                if (Directory.Exists(dst)) return Conflict("Tên đã tồn tại");
                Directory.Move(src, dst);
                return Json(new { ok = true });
            }
            return NotFound();
        }
    }
}
