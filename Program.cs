using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

var builder = WebApplication.CreateBuilder(args);

// ===================== Services =====================
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Hash mật khẩu cho NguoiDung (tự quản)
builder.Services.AddSingleton<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();

var app = builder.Build();

// ===================== Middleware =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===================== Seed dữ liệu =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>();

    // Tự động chạy migration nếu chưa có
    db.Database.Migrate();

    // Seed Admin mặc định (đổi mật khẩu sau khi đăng nhập)
    if (!db.NguoiDungs.Any(x => x.VaiTro == "Admin"))
    {
        var admin = new NguoiDung
        {
            HoTen = "Quản trị hệ thống",
            Email = "admin@local",
            VaiTro = "Admin",
            KichHoat = true
        };
        admin.MatKhauHash = hasher.HashPassword(admin, "123456");
        db.NguoiDungs.Add(admin);
        db.SaveChanges();
    }

    // Seed vài Chuyên mục mẫu nếu bảng đang rỗng
    if (!db.ChuyenMucs.Any())
    {
        db.ChuyenMucs.AddRange(
            new ChuyenMuc { Ten = "Tin tức" },
            new ChuyenMuc { Ten = "Sự kiện" },
            new ChuyenMuc { Ten = "Thông báo" },
            new ChuyenMuc { Ten = "Hướng dẫn" }
        );
        db.SaveChanges();
    }
}

// ===================== Routes =====================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
