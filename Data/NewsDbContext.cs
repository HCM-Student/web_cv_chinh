using Microsoft.EntityFrameworkCore;
using WEB_CV.Models;

namespace WEB_CV.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }

        public DbSet<ChuyenMuc> ChuyenMucs { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<BaiViet> BaiViets { get; set; }
        public DbSet<BinhLuan> BinhLuans { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BaiVietTag> BaiVietTags { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Khóa chính bảng nối N-N
            mb.Entity<BaiVietTag>().HasKey(x => new { x.BaiVietId, x.TagId });

            // BaiViet → ChuyenMuc (1-n)
            mb.Entity<BaiViet>()
              .HasOne(x => x.ChuyenMuc)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.ChuyenMucId)
              .OnDelete(DeleteBehavior.Restrict);

            // BaiViet → NguoiDung (tác giả) (1-n)
            mb.Entity<BaiViet>()
              .HasOne(x => x.TacGia)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.TacGiaId)
              .OnDelete(DeleteBehavior.Restrict);

            // BinhLuan → BaiViet (1-n)
            mb.Entity<BinhLuan>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BinhLuans)
              .HasForeignKey(x => x.BaiVietId);

            // N-N: BaiViet ↔ Tag
            mb.Entity<BaiVietTag>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.BaiVietId);

            mb.Entity<BaiVietTag>()
              .HasOne(x => x.Tag)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.TagId);
        }
    }
}
