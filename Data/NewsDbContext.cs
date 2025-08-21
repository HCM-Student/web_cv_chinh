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

            // ===== User =====
            // Email duy nhất để đăng nhập
            mb.Entity<NguoiDung>()
              .HasIndex(x => x.Email)
              .IsUnique();

            // ===== N-N: BaiViet <-> Tag (bảng nối) =====
            mb.Entity<BaiVietTag>().HasKey(x => new { x.BaiVietId, x.TagId });

            mb.Entity<BaiVietTag>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.BaiVietId)
              .OnDelete(DeleteBehavior.Cascade); // xoá bài -> xoá bản ghi nối

            mb.Entity<BaiVietTag>()
              .HasOne(x => x.Tag)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.TagId)
              .OnDelete(DeleteBehavior.Cascade); // xoá tag -> xoá bản ghi nối

            // ===== 1-n: BaiViet -> ChuyenMuc =====
            mb.Entity<BaiViet>()
              .HasOne(x => x.ChuyenMuc)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.ChuyenMucId)
              .OnDelete(DeleteBehavior.Restrict); // tránh xoá chuyên mục làm xoá bài

            // ===== 1-n: BaiViet -> NguoiDung (TacGia) =====
            mb.Entity<BaiViet>()
              .HasOne(x => x.TacGia)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.TacGiaId)
              .OnDelete(DeleteBehavior.Restrict); // tránh xoá tác giả làm xoá bài

            // ===== 1-n: BinhLuan -> BaiViet =====
            mb.Entity<BinhLuan>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BinhLuans)
              .HasForeignKey(x => x.BaiVietId)
              .OnDelete(DeleteBehavior.Cascade); // xoá bài -> xoá bình luận
        }
    }
}
