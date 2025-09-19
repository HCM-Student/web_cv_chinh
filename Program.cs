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
using WEB_CV.Models.Options;
using WEB_CV.Services;
using WEB_CV.Services.Backup;
using WEB_CV.Services.AI;
using WEB_CV.Background;

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

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Hash mật khẩu cho NguoiDung (tự quản)
builder.Services.AddSingleton<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();

// SignalR
builder.Services.AddSignalR();

// App services
builder.Services.AddScoped<ICaiDatService, CaiDatService>();
builder.Services.AddScoped<ISEOAnalysisService, SEOAnalysisService>();
builder.Services.AddScoped<IOnlineUserService, OnlineUserService>();
builder.Services.AddScoped<IScheduledPublishingService, ScheduledPublishingService>();
builder.Services.AddScoped<ISiteCounter, SiteCounter>();

// Backup services
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddHostedService<BackupScheduler>();

// Configuration options
builder.Services.Configure<WebsiteLinksOptions>(
    builder.Configuration.GetSection("WebsiteLinks"));

// AI Services
builder.Services.AddHttpClient<IAIWritingService, AIWritingService>();
builder.Services.AddScoped<IAIWritingService, AIWritingService>();
builder.Services.AddHttpClient<IProtonxSearch, ProtonxSearch>();
builder.Services.AddHttpContextAccessor();

// Background services
builder.Services.AddHostedService<ScheduledPublishingBackgroundService>();
builder.Services.AddHostedService<OnlineUserCleanupService>();

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

app.UseSession();

// Site tracking middleware (track visits and online users)
app.UseMiddleware<WEB_CV.Middleware.SiteTrackingMiddleware>();

// Online user tracking middleware
app.UseMiddleware<WEB_CV.Middleware.OnlineUserTrackingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Bảo vệ khu admin: tất cả controller trong /Admin yêu cầu Admin hoặc trưởng phòng roles
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/admin") && 
        !ctx.User.IsInRole("Admin") && 
        !ctx.User.IsInRole("TruongPhongPhatTrien") && 
        !ctx.User.IsInRole("TruongPhongNhanSu") && 
        !ctx.User.IsInRole("TruongPhongDuLieu"))
    {
        ctx.Response.StatusCode = 403; // Forbidden
        return;
    }
    await next();
});

// ===================== Seed dữ liệu =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>();
    var caiDatService = scope.ServiceProvider.GetRequiredService<ICaiDatService>();
    

    // Tự động chạy migration nếu chưa có
    db.Database.Migrate();

    // Seed Admin mặc định (đổi mật khẩu sau khi đăng nhập) - hệ thống cũ
    if (!db.NguoiDungs.Any(x => x.VaiTro == "Admin"))
    {
        var adminOld = new NguoiDung
        {
            HoTen = "Quản trị hệ thống",
            Email = "admin@local",
            VaiTro = "Admin",
            KichHoat = true
        };
        adminOld.MatKhauHash = hasher.HashPassword(adminOld, "123456");
        db.NguoiDungs.Add(adminOld);
        db.SaveChanges();
    }

    // Seed Staff mẫu - hệ thống cũ
    var existingStaff = db.NguoiDungs.FirstOrDefault(x => x.Email == "canbo@example.gov.vn");
    if (existingStaff != null)
    {
        // Cập nhật vai trò nếu đã tồn tại
        existingStaff.VaiTro = "Staff";
        db.SaveChanges();
    }
    else if (!db.NguoiDungs.Any(x => x.VaiTro == "Staff"))
    {
        var staffOld = new NguoiDung
        {
            HoTen = "Cán bộ nhân viên",
            Email = "canbo@example.gov.vn",
            VaiTro = "Staff",
            KichHoat = true
        };
        staffOld.MatKhauHash = hasher.HashPassword(staffOld, "CanBo@123!");
        db.NguoiDungs.Add(staffOld);
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

    // Seed dữ liệu mẫu cho Lịch công tác
    if (!db.WorkScheduleEvents.Any())
    {
        var mon = DateTime.Today.AddDays(-(7 + (int)DateTime.Today.DayOfWeek - 1) % 7); // về thứ 2 tuần hiện tại
        db.WorkScheduleEvents.AddRange(
            new WorkScheduleEvent { 
                Date = mon, 
                StartTime = new TimeSpan(8,0,0), 
                EndTime = new TimeSpan(9,30,0),
                Title = "Họp giao ban đầu tuần", 
                Location="Phòng A101", 
                Organization="Văn phòng Cục",
                Participants="Ban Lãnh đạo, Trưởng/Phó phòng", 
                Preparation="Văn phòng", 
                Contact="Nguyễn An", 
                Phone="0903xxx", 
                Email="an@example.gov.vn" 
            },
            new WorkScheduleEvent { 
                Date = mon.AddDays(2), 
                StartTime = new TimeSpan(14,0,0),
                Title = "Làm việc với Sở TN&MT tỉnh X", 
                Location="Trực tuyến", 
                Organization="Cục Chuyển đổi số",
                Participants="Tổ công tác chuyển đổi số", 
                Preparation="CNTT", 
                Contact="Trần Bình",
                Phone="0987xxx",
                Email="binh@example.gov.vn"
            },
            new WorkScheduleEvent { 
                Date = mon.AddDays(4), 
                StartTime = new TimeSpan(9,0,0), 
                EndTime = new TimeSpan(11,0,0),
                Title = "Hội thảo chuyển đổi số", 
                Location="Hội trường lớn", 
                Organization="Bộ TT&TT",
                Participants="Các đơn vị trực thuộc", 
                Preparation="Ban tổ chức", 
                Contact="Lê Minh",
                Phone="0912xxx",
                Email="minh@example.gov.vn"
            }
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

// SignalR Hubs
app.MapHub<WEB_CV.Hubs.ChatHub>("/hubs/chat");

app.Run();
