using System;
using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using WEB_CV.Data;
using WEB_CV.Models;
using WEB_CV.Services;

var builder = WebApplication.CreateBuilder(args);

// ===================== Services =====================

// Cấu hình upload file lớn
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

// DbContext
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Localization (VI/EN) – mặc định VI
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

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

// Hash mật khẩu cho NguoiDung (tự quản)
builder.Services.AddSingleton<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();

// App services
builder.Services.AddScoped<ICaiDatService, CaiDatService>();
builder.Services.AddScoped<ISEOAnalysisService, SEOAnalysisService>();
builder.Services.AddScoped<IScheduledPublishingService, ScheduledPublishingService>();

// AI Services
builder.Services.AddHttpClient<IAIWritingService, AIWritingService>();
builder.Services.AddScoped<IAIWritingService, AIWritingService>();
builder.Services.AddHttpContextAccessor();

// Background services
builder.Services.AddHostedService<ScheduledPublishingBackgroundService>();

// RequestLocalization (ưu tiên cookie)
var supportedCultures = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// Cấu hình timezone cho Việt Nam
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var viCulture = new CultureInfo("vi-VN");
    viCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
    options.DefaultRequestCulture = new RequestCulture(viCulture);
    options.SupportedCultures = new[] { viCulture, new CultureInfo("en-US") };
    options.SupportedUICultures = new[] { viCulture, new CultureInfo("en-US") };
});

var app = builder.Build();

// ===================== Middleware =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Ensure media directory exists
var mediaPath = Path.Combine(app.Environment.WebRootPath, "media");
if (!Directory.Exists(mediaPath))
{
    Directory.CreateDirectory(mediaPath);
}

app.UseStaticFiles();

// Áp dụng localization TRƯỚC routing
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===================== Seed dữ liệu =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>();
    var caiDatService = scope.ServiceProvider.GetRequiredService<ICaiDatService>();

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

    // Initialize default settings
    await caiDatService.InitializeDefaultSettingsAsync();
}

// ===================== Routes =====================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
