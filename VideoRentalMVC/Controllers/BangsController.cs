using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VideoRentalMVC.Models;

namespace VideoRentalMVC.Controllers
{
    [Authorize]
    public class BangsController : Controller
    {
        private readonly VideoRentalDbContext _context;
        private const int PageSize = 8;

        public BangsController(VideoRentalDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int page = 1)
        {
            var query = _context.Bangs
                .Include(b => b.Phim)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(b =>
                    b.TenBang.Contains(searchString) ||
                    b.TinhTrang.Contains(searchString) ||
                    (b.Phim != null && b.Phim.TenPhim.Contains(searchString)));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(b => b.TenBang),
                "status_asc" => query.OrderBy(b => b.TinhTrang),
                "status_desc" => query.OrderByDescending(b => b.TinhTrang),
                _ => query.OrderBy(b => b.TenBang)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = sortOrder == "name_desc" ? string.Empty : "name_desc";
            ViewBag.StatusSort = sortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var bang = await _context.Bangs.Include(b => b.Phim).FirstOrDefaultAsync(b => b.Id == id);
            if (bang == null) return NotFound();
            return View(bang);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.MaPhim = new SelectList(_context.Phims, "Id", "TenPhim");
            ViewBag.TinhTrangList = GetTinhTrangList();
            return View(new Bang { TinhTrang = "Có sẵn" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,MaPhim,TenBang,TinhTrang")] Bang bang)
        {
            if (ModelState.IsValid)
            {
                _context.Bangs.Add(bang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaPhim = new SelectList(_context.Phims, "Id", "TenPhim", bang.MaPhim);
            ViewBag.TinhTrangList = GetTinhTrangList();
            return View(bang);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var bang = await _context.Bangs.FindAsync(id);
            if (bang == null) return NotFound();
            ViewBag.MaPhim = new SelectList(_context.Phims, "Id", "TenPhim", bang.MaPhim);
            ViewBag.TinhTrangList = GetTinhTrangList();
            return View(bang);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhim,TenBang,TinhTrang")] Bang bang)
        {
            if (id != bang.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bangs.Any(e => e.Id == bang.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaPhim = new SelectList(_context.Phims, "Id", "TenPhim", bang.MaPhim);
            ViewBag.TinhTrangList = GetTinhTrangList();
            return View(bang);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var bang = await _context.Bangs.Include(b => b.Phim).FirstOrDefaultAsync(b => b.Id == id);
            if (bang == null) return NotFound();

            var relatedRentalsCount = await _context.Thues.AsNoTracking().CountAsync(t => t.MaBang == id.Value);
            ViewBag.CanDelete = relatedRentalsCount == 0;
            ViewBag.RelatedRentalsCount = relatedRentalsCount;

            return View(bang);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bang = await _context.Bangs.FindAsync(id);
            if (bang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy băng để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var hasRelatedRentals = await _context.Thues.AsNoTracking().AnyAsync(t => t.MaBang == id);
            if (hasRelatedRentals)
            {
                TempData["ErrorMessage"] = "Không thể xóa băng vì đã có phiếu thuê liên quan.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Bangs.Remove(bang);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa băng thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa băng do dữ liệu đang được sử dụng.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> GetTinhTrangList() =>
        [
            new SelectListItem("Có sẵn", "Có sẵn"),
            new SelectListItem("Đang thuê", "Đang thuê"),
            new SelectListItem("Hỏng", "Hỏng"),
            new SelectListItem("Mất", "Mất")
        ];
    }
}
