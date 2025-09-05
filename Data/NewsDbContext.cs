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
        public DbSet<CaiDat> CaiDats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ===== User =====
            mb.Entity<NguoiDung>()
              .HasIndex(x => x.Email)
              .IsUnique();

            // ===== 1-n: BaiViet -> ChuyenMuc =====
            mb.Entity<BaiViet>()
              .HasOne(x => x.ChuyenMuc)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.ChuyenMucId)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== 1-n: BaiViet -> NguoiDung (TacGia) =====
            mb.Entity<BaiViet>()
              .HasOne(x => x.TacGia)
              .WithMany(x => x.BaiViets)
              .HasForeignKey(x => x.TacGiaId)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== 1-n: BinhLuan -> BaiViet =====
            mb.Entity<BinhLuan>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BinhLuans)
              .HasForeignKey(x => x.BaiVietId)
              .OnDelete(DeleteBehavior.Cascade);

            // ===== N-N: BaiViet <-> Tag (bảng nối) =====
            mb.Entity<BaiVietTag>().HasKey(x => new { x.BaiVietId, x.TagId });

            mb.Entity<BaiVietTag>()
              .HasOne(x => x.BaiViet)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.BaiVietId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<BaiVietTag>()
              .HasOne(x => x.Tag)
              .WithMany(x => x.BaiVietTags)
              .HasForeignKey(x => x.TagId)
              .OnDelete(DeleteBehavior.Cascade);

            // ===== Ràng buộc đơn giản cho ChuyenMuc =====
           // ===== Ràng buộc ChuyenMuc =====
        mb.Entity<ChuyenMuc>(e =>
    {
        e.Property(x => x.Ten).IsRequired().HasMaxLength(200);
        e.Property(x => x.Slug).HasMaxLength(200);     // <— THÊM
        e.Property(x => x.MoTa).HasMaxLength(1000);    // <— THÊM
    });

            // ===== Ràng buộc CaiDat =====
            mb.Entity<CaiDat>(e =>
            {
                e.HasIndex(x => x.Key).IsUnique();
                e.Property(x => x.Key).IsRequired().HasMaxLength(50);
                e.Property(x => x.Value).IsRequired();
                e.Property(x => x.Type).HasDefaultValue("string");
                e.Property(x => x.NgayCapNhat).HasDefaultValueSql("GETDATE()");
            });

            // ===== Ràng buộc Message =====
            mb.Entity<Message>(e =>
            {
                e.Property(x => x.Id).IsRequired().HasMaxLength(50);
                e.Property(x => x.Content).IsRequired().HasMaxLength(2000);
                e.Property(x => x.SentAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.IsRead).HasDefaultValue(false);
                
                e.HasIndex(x => new { x.FromUserId, x.ToUserId });
                e.HasIndex(x => x.SentAtUtc);
            });

        }
    }
}
