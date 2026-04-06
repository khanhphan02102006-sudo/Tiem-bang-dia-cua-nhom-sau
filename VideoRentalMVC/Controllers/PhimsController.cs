using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalMVC.Models;

namespace VideoRentalMVC.Controllers
{
    [Authorize]
    public class PhimsController : Controller
    {
        private readonly VideoRentalDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const int PageSize = 8;

        public PhimsController(VideoRentalDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task<string?> SavePosterAsync(IFormFile? posterUpload)
        {
            if (posterUpload == null || posterUpload.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "posters");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(posterUpload.FileName);
            var fileName = $"poster_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await posterUpload.CopyToAsync(stream);

            return $"/uploads/posters/{fileName}";
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int page = 1)
        {
            var query = _context.Phims.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p =>
                    p.TenPhim.Contains(searchString) ||
                    p.TheLoai.Contains(searchString) ||
                    p.NuocSanXuat.Contains(searchString));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(p => p.TenPhim),
                "year_asc" => query.OrderBy(p => p.NamSanXuat),
                "year_desc" => query.OrderByDescending(p => p.NamSanXuat),
                "price_asc" => query.OrderBy(p => p.GiaVon),
                "price_desc" => query.OrderByDescending(p => p.GiaVon),
                _ => query.OrderBy(p => p.TenPhim)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = sortOrder == "name_desc" ? string.Empty : "name_desc";
            ViewBag.YearSort = sortOrder == "year_asc" ? "year_desc" : "year_asc";
            ViewBag.PriceSort = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(items);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var phim = await _context.Phims.FirstOrDefaultAsync(m => m.Id == id);
            if (phim == null) return NotFound();
            return View(phim);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,TenPhim,TheLoai,NamSanXuat,NuocSanXuat,MoTa,PhimBoLe,GiaVon,AnhBiaUrl")] Phim phim, IFormFile? posterUpload)
        {
            var uploadedPoster = await SavePosterAsync(posterUpload);
            if (!string.IsNullOrWhiteSpace(uploadedPoster))
            {
                phim.AnhBiaUrl = uploadedPoster;
            }

            if (ModelState.IsValid)
            {
                _context.Add(phim);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phim thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(phim);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var phim = await _context.Phims.FindAsync(id);
            if (phim == null) return NotFound();
            return View(phim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenPhim,TheLoai,NamSanXuat,NuocSanXuat,MoTa,PhimBoLe,GiaVon,AnhBiaUrl")] Phim phim, IFormFile? posterUpload)
        {
            if (id != phim.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var phimToUpdate = await _context.Phims.FirstOrDefaultAsync(p => p.Id == id);
                    if (phimToUpdate == null)
                    {
                        return NotFound();
                    }

                    var uploadedPoster = await SavePosterAsync(posterUpload);
                    if (!string.IsNullOrWhiteSpace(uploadedPoster))
                    {
                        phim.AnhBiaUrl = uploadedPoster;
                    }

                    phimToUpdate.TenPhim = phim.TenPhim;
                    phimToUpdate.TheLoai = phim.TheLoai;
                    phimToUpdate.NamSanXuat = phim.NamSanXuat;
                    phimToUpdate.NuocSanXuat = phim.NuocSanXuat;
                    phimToUpdate.MoTa = phim.MoTa;
                    phimToUpdate.PhimBoLe = phim.PhimBoLe;
                    phimToUpdate.GiaVon = phim.GiaVon;
                    phimToUpdate.AnhBiaUrl = phim.AnhBiaUrl;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật phim thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Phims.Any(e => e.Id == phim.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(phim);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var phim = await _context.Phims.FirstOrDefaultAsync(m => m.Id == id);
            if (phim == null) return NotFound();
            return View(phim);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasRelatedBang = await _context.Bangs.AnyAsync(b => b.MaPhim == id);
            if (hasRelatedBang)
            {
                TempData["ErrorMessage"] = "Không thể xóa phim vì vẫn còn băng liên quan.";
                return RedirectToAction(nameof(Index));
            }

            var phim = await _context.Phims.FindAsync(id);
            if (phim != null)
            {
                _context.Phims.Remove(phim);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa phim thành công.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
