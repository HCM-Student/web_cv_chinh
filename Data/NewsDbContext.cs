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
        public DbSet<LienHe> LienHes { get; set; }
        public DbSet<SEOAnalysis> SEOAnalyses { get; set; }
        public DbSet<OnlineUser> OnlineUsers { get; set; }

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

            // ===== Ràng buộc ChuyenMuc =====
            mb.Entity<ChuyenMuc>(e =>
            {
                e.Property(x => x.Ten).IsRequired().HasMaxLength(200);
                e.Property(x => x.Slug).HasMaxLength(200);
                e.Property(x => x.MoTa).HasMaxLength(1000);
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

            // ===== LienHe =====
            mb.Entity<LienHe>(e =>
            {
                e.ToTable("LienHes"); // đồng bộ tên bảng với DbSet
                e.Property(x => x.HoTen).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.Property(x => x.SoDienThoai).IsRequired().HasMaxLength(20);
                e.Property(x => x.TieuDe).HasMaxLength(200);
                e.Property(x => x.NoiDung).IsRequired();
                e.Property(x => x.NgayGui).HasDefaultValueSql("GETDATE()");
                e.HasIndex(x => x.NgayGui);
            });

            // ===== SEOAnalysis =====
            mb.Entity<SEOAnalysis>(e =>
            {
                e.Property(x => x.TongDiem).HasDefaultValue(0);
                e.Property(x => x.DiemTieuDe).HasDefaultValue(0);
                e.Property(x => x.DiemTomTat).HasDefaultValue(0);
                e.Property(x => x.DiemNoiDung).HasDefaultValue(0);
                e.Property(x => x.DiemTuKhoa).HasDefaultValue(0);
                e.Property(x => x.DiemCauTruc).HasDefaultValue(0);
                e.Property(x => x.DiemHinhAnh).HasDefaultValue(0);
                e.Property(x => x.DiemLienKet).HasDefaultValue(0);
                e.Property(x => x.DiemDoDai).HasDefaultValue(0);
                e.Property(x => x.NgayPhanTich).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.DaXuLy).HasDefaultValue(false);
                
                // Foreign key relationship
                e.HasOne(x => x.BaiViet)
                 .WithMany()
                 .HasForeignKey(x => x.BaiVietId)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                e.HasIndex(x => x.BaiVietId);
                e.HasIndex(x => x.NgayPhanTich);
            });

            // ===== OnlineUser =====
            mb.Entity<OnlineUser>(e =>
            {
                e.Property(x => x.SessionId).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).HasMaxLength(150);
                e.Property(x => x.HoTen).HasMaxLength(100);
                e.Property(x => x.IpAddress).HasMaxLength(50);
                e.Property(x => x.UserAgent).HasMaxLength(200);
                e.Property(x => x.LastSeen).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.IsActive).HasDefaultValue(true);
                
                e.HasIndex(x => x.SessionId);
                e.HasIndex(x => x.LastSeen);
                e.HasIndex(x => x.IsActive);
            });
        }
    }
}
