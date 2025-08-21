using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC
builder.Services.AddControllersWithViews();

// Cookie Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// PasswordHasher cho NguoiDung
builder.Services.AddSingleton<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();

var app = builder.Build();

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

// Seed DB + tạo admin mặc định nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>();

    db.Database.Migrate();

    if (!db.NguoiDungs.Any(x => x.VaiTro == "Admin"))
    {
        var admin = new NguoiDung
        {
            HoTen = "Quản trị hệ thống",
            Email = "admin@local",
            VaiTro = "Admin",
            KichHoat = true
        };
        admin.MatKhauHash = hasher.HashPassword(admin, "123456"); // 👉 Nhớ đổi sau khi đăng nhập
        db.NguoiDungs.Add(admin);
        db.SaveChanges();
    }
}

// Routes (map Areas trước)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
