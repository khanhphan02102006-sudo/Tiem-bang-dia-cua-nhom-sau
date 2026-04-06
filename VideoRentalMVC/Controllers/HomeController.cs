using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VideoRentalMVC.Models;
using VideoRentalMVC.ViewModels;

namespace VideoRentalMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly VideoRentalDbContext _context;

        public HomeController(VideoRentalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var placeholderPoster = Url.Content("~/images/poster-placeholder.svg");

            var featuredPoster = await _context.Phims
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.AnhBiaUrl))
                .OrderByDescending(p => p.Id)
                .Select(p => new { p.TenPhim, p.AnhBiaUrl })
                .FirstOrDefaultAsync();

            var featuredPosters = await _context.Phims
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Select(p => new HomePosterCardViewModel
                {
                    Id = p.Id,
                    TenPhim = p.TenPhim,
                    AnhBiaUrl = string.IsNullOrWhiteSpace(p.AnhBiaUrl) ? placeholderPoster : p.AnhBiaUrl!
                })
                .Take(6)
                .ToListAsync();

            var topFilms = await _context.Thues
                .AsNoTracking()
                .Join(_context.Bangs.AsNoTracking(), t => t.MaBang, b => b.Id, (t, b) => new { t, b })
                .Join(_context.Phims.AsNoTracking(), tb => tb.b.MaPhim, p => p.Id, (tb, p) => new { tb.t, p })
                .GroupBy(x => new { x.p.Id, x.p.TenPhim, x.p.TheLoai, x.p.AnhBiaUrl })
                .Select(g => new HomeTrendingFilmViewModel
                {
                    Id = g.Key.Id,
                    TenPhim = g.Key.TenPhim,
                    TheLoai = g.Key.TheLoai,
                    AnhBiaUrl = string.IsNullOrWhiteSpace(g.Key.AnhBiaUrl) ? placeholderPoster : g.Key.AnhBiaUrl!,
                    SoLanThue = g.Count()
                })
                .OrderByDescending(x => x.SoLanThue)
                .ThenBy(x => x.TenPhim)
                .Take(4)
                .ToListAsync();

            var topGenres = await _context.Phims
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.TheLoai))
                .GroupBy(p => p.TheLoai)
                .Select(g => new HomeGenreCardViewModel
                {
                    TheLoai = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.TheLoai)
                .Take(6)
                .ToListAsync();

            var recentRentals = await _context.Thues
                .AsNoTracking()
                .OrderByDescending(t => t.Id)
                .Select(t => new HomeRentalCardViewModel
                {
                    Id = t.Id,
                    TenBang = t.MaBangNavigation != null ? t.MaBangNavigation.TenBang : "-",
                    TenKhach = t.MaKhachNavigation != null ? t.MaKhachNavigation.TenKhach : "-",
                    HanTra = t.HanTra,
                    NgayTra = t.NgayTra,
                    IsOverdue = t.NgayTra == null && t.HanTra < now,
                    IsNearDue = t.NgayTra == null && t.HanTra >= now && t.HanTra <= now.AddDays(1)
                })
                .Take(5)
                .ToListAsync();

            var tongPhim = await _context.Phims.CountAsync();
            var tongBang = await _context.Bangs.CountAsync();
            var dangThue = await _context.Thues.CountAsync(t => t.NgayTra == null);
            var quaHan = await _context.Thues.CountAsync(t => t.NgayTra == null && t.HanTra < now);
            var doanhThuThang = await _context.Thues
                .Where(t => t.DaTraTienThue && t.NgayThue.Month == now.Month && t.NgayThue.Year == now.Year)
                .SumAsync(t => (decimal?)t.TongTien) ?? 0m;

            if (!featuredPosters.Any())
            {
                featuredPosters.Add(new HomePosterCardViewModel
                {
                    Id = 0,
                    TenPhim = "Chưa có phim",
                    AnhBiaUrl = placeholderPoster
                });
            }

            if (!topFilms.Any())
            {
                topFilms.Add(new HomeTrendingFilmViewModel
                {
                    Id = 0,
                    TenPhim = "Chưa có dữ liệu",
                    TheLoai = "-",
                    AnhBiaUrl = placeholderPoster,
                    SoLanThue = 0
                });
            }

            if (!topGenres.Any())
            {
                topGenres.Add(new HomeGenreCardViewModel
                {
                    TheLoai = "Chưa có dữ liệu",
                    SoLuong = 0
                });
            }

            if (!recentRentals.Any())
            {
                recentRentals.Add(new HomeRentalCardViewModel
                {
                    Id = 0,
                    TenBang = "Chưa có phiếu",
                    TenKhach = "-",
                    HanTra = now
                });
            }

            ViewBag.FeaturedPosterUrl = featuredPoster?.AnhBiaUrl ?? placeholderPoster;
            ViewBag.FeaturedPosterTitle = featuredPoster?.TenPhim ?? "Poster nổi bật";
            ViewBag.FeaturedPosters = featuredPosters;
            ViewBag.TopFilms = topFilms;
            ViewBag.TopGenres = topGenres;
            ViewBag.RecentRentals = recentRentals;

            var model = new DashboardViewModel
            {
                TongPhim = tongPhim,
                TongBang = tongBang,
                DangThue = dangThue,
                QuaHan = quaHan,
                DoanhThuThang = doanhThuThang
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
