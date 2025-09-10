using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;

namespace WEB_CV.Controllers
{
    [ApiController]
    public class SeoController : Controller
    {
        private readonly NewsDbContext _db;

        public SeoController(NewsDbContext db)
        {
            _db = db;
        }

        // ================ robots.txt ================
        [HttpGet]
        [Route("robots.txt")]
        public IActionResult Robots()
        {
            var sitemapUrl = GenerateAbsoluteUrl("Sitemap");
            var content = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Allow: /")
                .AppendLine($"Sitemap: {sitemapUrl}")
                .ToString();

            return Content(content, "text/plain", Encoding.UTF8);
        }

        // ================ sitemap.xml (index) ================
        [HttpGet]
        [Route("sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            Response.ContentType = "application/xml";

            var baseUrl = GetBaseUrl();

            var categories = await _db.ChuyenMucs
                .AsNoTracking()
                .Select(c => new { c.Id, c.Slug })
                .ToListAsync();

            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");

                // Root pages
                WriteSitemapEntry(writer, $"{baseUrl}/sitemap-static.xml");

                // Per-category sitemaps
                foreach (var c in categories)
                {
                    var path = string.IsNullOrWhiteSpace(c.Slug)
                        ? $"{baseUrl}/sitemap-category-{c.Id}.xml"
                        : $"{baseUrl}/sitemap-category-{c.Slug}.xml";
                    WriteSitemapEntry(writer, path);
                }

                // All posts sitemap
                WriteSitemapEntry(writer, $"{baseUrl}/sitemap-posts.xml");

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            stream.Position = 0;
            return File(stream.ToArray(), "application/xml; charset=utf-8");
        }

        // ================ static pages sitemap ================
        [HttpGet]
        [Route("sitemap-static.xml")]
        public IActionResult SitemapStatic()
        {
            var baseUrl = GetBaseUrl();
            var urls = new List<string>
            {
                $"{baseUrl}/",
                $"{baseUrl}/gioi-thieu",
                $"{baseUrl}/tin-tuc",
                $"{baseUrl}/lien-he",
                $"{baseUrl}/thiet-bi",
                $"{baseUrl}/tuyen-dung",
                $"{baseUrl}/bao-gia"
            };

            return BuildUrlset(urls);
        }

        // ================ per-category sitemap ================
        [HttpGet]
        [Route("sitemap-category-{key}.xml")]
        public async Task<IActionResult> SitemapCategory(string key)
        {
            var baseUrl = GetBaseUrl();

            var query = _db.ChuyenMucs.AsNoTracking().AsQueryable();
            WEB_CV.Models.ChuyenMuc? category;

            if (int.TryParse(key, out var parsedId))
            {
                category = await query.FirstOrDefaultAsync(c => c.Id == parsedId || c.Slug == key);
            }
            else
            {
                category = await query.FirstOrDefaultAsync(c => c.Slug == key);
            }

            if (category == null)
                return NotFound();

            var posts = await _db.BaiViets
                .AsNoTracking()
                .Where(b => b.ChuyenMucId == category.Id)
                .OrderByDescending(b => b.NgayDang)
                .Select(b => new { b.Id, b.NgayDang })
                .ToListAsync();

            var urls = posts.Select(p => $"{baseUrl}/bai-viet/{p.Id}").ToList();
            return BuildUrlset(urls);
        }

        // ================ all posts sitemap ================
        [HttpGet]
        [Route("sitemap-posts.xml")]
        public async Task<IActionResult> SitemapPosts()
        {
            var baseUrl = GetBaseUrl();
            var posts = await _db.BaiViets
                .AsNoTracking()
                .OrderByDescending(b => b.NgayDang)
                .Select(b => new { b.Id })
                .ToListAsync();

            var urls = posts.Select(p => $"{baseUrl}/bai-viet/{p.Id}").ToList();
            return BuildUrlset(urls);
        }

        // ================ RSS feed ================
        [HttpGet]
        [Route("rss.xml")]
        public async Task<IActionResult> Rss()
        {
            Response.ContentType = "application/rss+xml";

            var baseUrl = GetBaseUrl();
            var posts = await _db.BaiViets
                .Include(b => b.ChuyenMuc)
                .AsNoTracking()
                .OrderByDescending(b => b.NgayDang)
                .Take(50)
                .ToListAsync();

            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");

                writer.WriteStartElement("channel");
                writer.WriteElementString("title", "Tin tức");
                writer.WriteElementString("link", baseUrl);
                writer.WriteElementString("description", "Bản tin mới nhất");

                foreach (var p in posts)
                {
                    writer.WriteStartElement("item");
                    writer.WriteElementString("title", p.TieuDe);
                    writer.WriteElementString("link", $"{baseUrl}/bai-viet/{p.Id}");
                    if (!string.IsNullOrEmpty(p.TomTat))
                        writer.WriteElementString("description", p.TomTat);
                    writer.WriteElementString("guid", $"{baseUrl}/bai-viet/{p.Id}");
                    writer.WriteElementString("pubDate", p.NgayDang.ToUniversalTime().ToString("r"));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // channel
                writer.WriteEndElement(); // rss
                writer.WriteEndDocument();
            }

            stream.Position = 0;
            return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }

        // ================ helpers ================
        private string GetBaseUrl()
        {
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        private string GenerateAbsoluteUrl(string actionName)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}/{(actionName == "Sitemap" ? "sitemap.xml" : actionName)}";
        }

        private static void WriteSitemapEntry(XmlWriter writer, string loc)
        {
            writer.WriteStartElement("sitemap");
            writer.WriteElementString("loc", loc);
            writer.WriteEndElement();
        }

        private IActionResult BuildUrlset(List<string> urls)
        {
            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var url in urls)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", url);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            stream.Position = 0;
            return File(stream.ToArray(), "application/xml; charset=utf-8");
        }
    }
}


