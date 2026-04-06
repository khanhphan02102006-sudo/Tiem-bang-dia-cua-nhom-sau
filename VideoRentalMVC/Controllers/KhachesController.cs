using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalMVC.Models;

namespace VideoRentalMVC.Controllers
{
    [Authorize]
    public class KhachesController : Controller
    {
        private readonly VideoRentalDbContext _context;
        private const int PageSize = 8;

        public KhachesController(VideoRentalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int page = 1)
        {
            var query = _context.Khachs.AsNoTracking().AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(k => k.IdentityUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(k =>
                    k.TenKhach.Contains(searchString) ||
                    k.DiaChi.Contains(searchString) ||
                    k.DienThoai.Contains(searchString));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(k => k.TenKhach),
                "phone_asc" => query.OrderBy(k => k.DienThoai),
                "phone_desc" => query.OrderByDescending(k => k.DienThoai),
                _ => query.OrderBy(k => k.TenKhach)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = sortOrder == "name_desc" ? string.Empty : "name_desc";
            ViewBag.PhoneSort = sortOrder == "phone_asc" ? "phone_desc" : "phone_asc";
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null) return NotFound();
                var adminKhach = await _context.Khachs.FirstOrDefaultAsync(m => m.Id == id);
                if (adminKhach == null) return NotFound();
                return View(adminKhach);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ownKhach = await _context.Khachs.FirstOrDefaultAsync(m => m.IdentityUserId == userId);
            if (ownKhach == null) return NotFound();

            if (id.HasValue && id.Value != ownKhach.Id)
            {
                return Forbid();
            }

            return View(ownKhach);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,TenKhach,DiaChi,DienThoai")] Khach khach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(khach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khach);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khach = await _context.Khachs.FindAsync(id);
            if (khach == null) return NotFound();

            return View(khach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenKhach,DiaChi,DienThoai")] Khach khach)
        {
            if (id != khach.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingKhach = await _context.Khachs.FindAsync(id);
                if (existingKhach == null) return NotFound();

                existingKhach.TenKhach = khach.TenKhach;
                existingKhach.DiaChi = khach.DiaChi;
                existingKhach.DienThoai = khach.DienThoai;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Khachs.Any(e => e.Id == khach.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khach);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var khach = await _context.Khachs.FirstOrDefaultAsync(m => m.Id == id);
            if (khach == null) return NotFound();

            var relatedRentalsCount = await _context.Thues.AsNoTracking().CountAsync(t => t.MaKhach == id.Value);
            ViewBag.CanDelete = relatedRentalsCount == 0;
            ViewBag.RelatedRentalsCount = relatedRentalsCount;

            return View(khach);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khach = await _context.Khachs.FindAsync(id);
            if (khach == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var hasRelatedRentals = await _context.Thues.AsNoTracking().AnyAsync(t => t.MaKhach == id);
            if (hasRelatedRentals)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách vì đã có phiếu thuê liên quan.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Khachs.Remove(khach);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa khách thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách do dữ liệu đang được sử dụng.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
