using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoRentalMVC.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Ensure UTF-8 text encoding
    var stringFormatter = new Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter();
    options.OutputFormatters.Add(stringFormatter);
});
builder.Services.AddDbContext<VideoRentalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("VideoRentalConnection"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.Configure<RentalSettings>(builder.Configuration.GetSection("RentalSettings"));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<VideoRentalDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Set UTF-8 encoding header for all HTML responses
app.Use(async (context, next) =>
{
    if (context.Request.Path.HasValue)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
    }
    await next();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoRentalDbContext>();
    db.Database.Migrate();

    var rentalSettings = scope.ServiceProvider.GetRequiredService<IOptions<RentalSettings>>().Value;
    await NormalizeLegacyRentalsAsync(db, rentalSettings);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedIdentityAsync(userManager, roleManager);
    await SeedDemoDataAsync(db, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static async Task NormalizeLegacyRentalsAsync(VideoRentalDbContext db, RentalSettings settings)
{
    var donGiaNgay = settings.DonGiaNgay <= 0 ? 10000m : settings.DonGiaNgay;
    var phiTreHanMoiNgay = settings.PhiTreHanMoiNgay <= 0 ? 5000m : settings.PhiTreHanMoiNgay;
    var soNgayHanMacDinh = settings.SoNgayHanMacDinh <= 0 ? 3 : settings.SoNgayHanMacDinh;

    var rows = await db.Thues.ToListAsync();
    var changed = false;

    foreach (var row in rows)
    {
        if (row.HanTra == default)
        {
            row.HanTra = row.NgayThue.Date.AddDays(soNgayHanMacDinh);
            changed = true;
        }

        var soNgayDuKien = Math.Max(1, (row.HanTra.Date - row.NgayThue.Date).Days + 1);
        var phiCoBan = soNgayDuKien * donGiaNgay;
        var ngayTinhTre = (row.NgayTra ?? DateTime.Today).Date;
        var soNgayTre = Math.Max(0, (ngayTinhTre - row.HanTra.Date).Days);
        var phiTre = soNgayTre * phiTreHanMoiNgay;
        var tongTien = phiCoBan + phiTre;

        if (row.PhiTreHan != phiTre || row.TongTien != tongTien)
        {
            row.PhiTreHan = phiTre;
            row.TongTien = tongTien;
            changed = true;
        }
    }

    if (changed)
    {
        await db.SaveChangesAsync();
    }
}

static async Task SeedIdentityAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    const string adminRole = "Admin";
    const string customerRole = "Customer";
    const string adminUserName = "admin";
    const string adminPassword = "admin@123";

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    if (!await roleManager.RoleExistsAsync(customerRole))
    {
        await roleManager.CreateAsync(new IdentityRole(customerRole));
    }

    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminUserName,
            Email = "admin@videorental.local",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Cannot create default admin account: {errors}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}

static async Task SeedDemoDataAsync(VideoRentalDbContext db, UserManager<IdentityUser> userManager)
{
    var demoUsers = new[]
    {
        new { UserName = "huy.demo", Email = "huy.demo@videorental.local", Password = "Demo@1234", TenKhach = "Chu Quang Huy", DienThoai = "0901000101", DiaChi = "Hà Nội" },
        new { UserName = "lan.demo", Email = "lan.demo@videorental.local", Password = "Demo@1234", TenKhach = "Nguyễn Mỹ Lan", DienThoai = "0901000102", DiaChi = "TP.HCM" },
        new { UserName = "minh.demo", Email = "minh.demo@videorental.local", Password = "Demo@1234", TenKhach = "Trần Đức Minh", DienThoai = "0901000103", DiaChi = "Đà Nẵng" }
    };

    foreach (var item in demoUsers)
    {
        var user = await userManager.FindByNameAsync(item.UserName);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = item.UserName,
                Email = item.Email,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(user, item.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join("; ", createUserResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Cannot create demo user '{item.UserName}': {errors}");
            }

            await userManager.AddToRoleAsync(user, "Customer");
        }

        var khach = await db.Khachs.FirstOrDefaultAsync(k => k.IdentityUserId == user.Id);
        if (khach == null)
        {
            db.Khachs.Add(new Khach
            {
                TenKhach = item.TenKhach,
                DiaChi = item.DiaChi,
                DienThoai = item.DienThoai,
                IdentityUserId = user.Id
            });
        }
    }

    await db.SaveChangesAsync();

    var demoFilms = new[]
    {
        new Phim { TenPhim = "Interstellar", TheLoai = "Sci-Fi", NamSanXuat = 2014, NuocSanXuat = "USA", MoTa = "Du hành không gian và thời gian.", PhimBoLe = false, GiaVon = 150000m, AnhBiaUrl = "https://picsum.photos/seed/interstellar/600/900" },
        new Phim { TenPhim = "Parasite", TheLoai = "Drama", NamSanXuat = 2019, NuocSanXuat = "Korea", MoTa = "Bi hài kịch xã hội.", PhimBoLe = false, GiaVon = 120000m, AnhBiaUrl = "https://picsum.photos/seed/parasite/600/900" },
        new Phim { TenPhim = "Your Name", TheLoai = "Anime", NamSanXuat = 2016, NuocSanXuat = "Japan", MoTa = "Câu chuyện hoán đổi thân xác.", PhimBoLe = false, GiaVon = 100000m, AnhBiaUrl = "https://picsum.photos/seed/yourname/600/900" },
        new Phim { TenPhim = "The Dark Knight", TheLoai = "Action", NamSanXuat = 2008, NuocSanXuat = "USA", MoTa = "Biểu tượng siêu anh hùng.", PhimBoLe = false, GiaVon = 140000m, AnhBiaUrl = "https://picsum.photos/seed/darkknight/600/900" }
    };

    foreach (var film in demoFilms)
    {
        if (!await db.Phims.AnyAsync(p => p.TenPhim == film.TenPhim))
        {
            db.Phims.Add(film);
        }
    }

    await db.SaveChangesAsync();

    var filmMap = await db.Phims
        .Where(p => demoFilms.Select(x => x.TenPhim).Contains(p.TenPhim))
        .ToDictionaryAsync(p => p.TenPhim, p => p.Id);

    var demoBangs = new[]
    {
        new { TenBang = "INT-001", TinhTrang = "Có sẵn", MaPhim = filmMap["Interstellar"] },
        new { TenBang = "PAR-001", TinhTrang = "Đang thuê", MaPhim = filmMap["Parasite"] },
        new { TenBang = "YRN-001", TinhTrang = "Có sẵn", MaPhim = filmMap["Your Name"] },
        new { TenBang = "TDK-001", TinhTrang = "Có sẵn", MaPhim = filmMap["The Dark Knight"] }
    };

    foreach (var bang in demoBangs)
    {
        if (!await db.Bangs.AnyAsync(b => b.TenBang == bang.TenBang))
        {
            db.Bangs.Add(new Bang
            {
                TenBang = bang.TenBang,
                TinhTrang = bang.TinhTrang,
                MaPhim = bang.MaPhim
            });
        }
    }

    await db.SaveChangesAsync();

    var khachIds = await db.Khachs
        .Where(k => k.TenKhach == "Chu Quang Huy" || k.TenKhach == "Nguyễn Mỹ Lan" || k.TenKhach == "Trần Đức Minh")
        .Select(k => k.Id)
        .ToListAsync();

    if (!khachIds.Any())
    {
        return;
    }

    var bangIds = await db.Bangs
        .Where(b => b.TenBang == "PAR-001" || b.TenBang == "INT-001" || b.TenBang == "YRN-001")
        .Select(b => b.Id)
        .ToListAsync();

    if (!bangIds.Any())
    {
        return;
    }

    if (!await db.Thues.AnyAsync())
    {
        var baseDate = DateTime.Today.AddDays(-3);
        for (var i = 0; i < Math.Min(3, bangIds.Count); i++)
        {
            var ngayThue = baseDate.AddDays(i);
            var hanTra = ngayThue.AddDays(3);
            var daTra = i == 0;
            var ngayTra = daTra ? hanTra.AddDays(-1) : (DateTime?)null;

            db.Thues.Add(new Thue
            {
                NgayThue = ngayThue,
                HanTra = hanTra,
                NgayTra = ngayTra,
                DaTraTienThue = daTra,
                PhiTreHan = 0m,
                TongTien = 40000m,
                MaBang = bangIds[i],
                MaKhach = khachIds[i % khachIds.Count]
            });
        }

        await db.SaveChangesAsync();
    }
}
