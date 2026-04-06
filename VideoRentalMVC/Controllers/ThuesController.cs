using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoRentalMVC.Models;

namespace VideoRentalMVC.Controllers
{
    [Authorize]
    public class ThuesController : Controller
    {
        private readonly VideoRentalDbContext _context;
        private readonly RentalSettings _rentalSettings;

        public ThuesController(VideoRentalDbContext context, IOptions<RentalSettings> rentalSettings)
        {
            _context = context;
            _rentalSettings = rentalSettings.Value;
        }

        public async Task<IActionResult> Index(string? searchString, string? sortOrder, string? statusFilter, int page = 1, int pageSize = 8)
        {
            if (pageSize is < 5 or > 50)
            {
                pageSize = 8;
            }

            var query = _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(t => t.MaKhachNavigation != null && t.MaKhachNavigation.IdentityUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(t =>
                    (t.MaBangNavigation != null && t.MaBangNavigation.TenBang.Contains(searchString)) ||
                    (t.MaKhachNavigation != null && t.MaKhachNavigation.TenKhach.Contains(searchString)));
            }

            query = statusFilter switch
            {
                "dangthue" => query.Where(t => t.NgayTra == null && t.HanTra >= DateTime.Today),
                "saptoihan" => query.Where(t => t.NgayTra == null && t.HanTra >= DateTime.Today && EF.Functions.DateDiffDay(DateTime.Today, t.HanTra) <= 1),
                "quahan" => query.Where(t => t.NgayTra == null && t.HanTra < DateTime.Today),
                "datra" => query.Where(t => t.NgayTra != null),
                _ => query
            };

            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(t => t.NgayThue),
                "date_desc" => query.OrderByDescending(t => t.NgayThue),
                "remain_asc" => query.OrderBy(t => t.HanTra),
                "remain_desc" => query.OrderByDescending(t => t.HanTra),
                "fee_asc" => query.OrderBy(t => t.PhiTreHan),
                "fee_desc" => query.OrderByDescending(t => t.PhiTreHan),
                "total_asc" => query.OrderBy(t => t.TongTien),
                "total_desc" => query.OrderByDescending(t => t.TongTien),
                _ => query.OrderByDescending(t => t.NgayThue)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.RemainSort = sortOrder == "remain_asc" ? "remain_desc" : "remain_asc";
            ViewBag.FeeSort = sortOrder == "fee_asc" ? "fee_desc" : "fee_asc";
            ViewBag.TotalSort = sortOrder == "total_asc" ? "total_desc" : "total_asc";
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var thue = await _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thue == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (thue.MaKhachNavigation?.IdentityUserId != userId)
                {
                    return Forbid();
                }
            }

            return View(thue);
        }

        public async Task<IActionResult> Create()
        {
            var isAdmin = User.IsInRole("Admin");
            ViewBag.IsAdmin = isAdmin;

            var availableBangs = await _context.Bangs
                .Where(b => b.TinhTrang != "Đang thuê" && !_context.Thues.Any(t => t.MaBang == b.Id && t.NgayTra == null))
                .OrderBy(b => b.TenBang)
                .ToListAsync();

            ViewBag.MaBang = new SelectList(availableBangs, "Id", "TenBang");

            if (isAdmin)
            {
                ViewBag.MaKhach = new SelectList(_context.Khachs.OrderBy(k => k.TenKhach), "Id", "TenKhach");
            }

            return View(new Thue
            {
                NgayThue = DateTime.Today,
                HanTra = DateTime.Today.AddDays(_rentalSettings.SoNgayHanMacDinh <= 0 ? 3 : _rentalSettings.SoNgayHanMacDinh)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaBang,MaKhach,NgayThue,HanTra,NgayTra,DaTraTienThue")] Thue thue)
        {
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                var ownKhach = await GetCurrentKhachAsync();
                if (ownKhach == null)
                {
                    return Forbid();
                }

                thue.MaKhach = ownKhach.Id;
                thue.NgayTra = null;
                thue.DaTraTienThue = false;
            }

            await ValidateThueAsync(thue);
            TinhTien(thue);

            if (ModelState.IsValid)
            {
                _context.Add(thue);

                var bang = await _context.Bangs.FindAsync(thue.MaBang);
                if (bang != null)
                {
                    bang.TinhTrang = "Đang thuê";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo phiếu thuê thành công.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCreateDropdownsAsync(thue, isAdmin);
            return View(thue);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var thue = await _context.Thues.FindAsync(id);
            if (thue == null) return NotFound();

            ViewBag.MaBang = new SelectList(_context.Bangs.OrderBy(b => b.TenBang), "Id", "TenBang", thue.MaBang);
            ViewBag.MaKhach = new SelectList(_context.Khachs.OrderBy(k => k.TenKhach), "Id", "TenKhach", thue.MaKhach);
            return View(thue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaBang,MaKhach,NgayThue,HanTra,NgayTra,DaTraTienThue")] Thue thue)
        {
            if (id != thue.Id) return NotFound();

            await ValidateThueAsync(thue);
            TinhTien(thue);

            if (ModelState.IsValid)
            {
                var existing = await _context.Thues.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                if (existing == null) return NotFound();

                _context.Update(thue);

                if (existing.MaBang != thue.MaBang)
                {
                    var oldBang = await _context.Bangs.FindAsync(existing.MaBang);
                    if (oldBang != null) oldBang.TinhTrang = "Có sẵn";
                }

                var newBang = await _context.Bangs.FindAsync(thue.MaBang);
                if (newBang != null)
                {
                    newBang.TinhTrang = thue.NgayTra == null ? "Đang thuê" : "Có sẵn";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật phiếu thuê thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaBang = new SelectList(_context.Bangs.OrderBy(b => b.TenBang), "Id", "TenBang", thue.MaBang);
            ViewBag.MaKhach = new SelectList(_context.Khachs.OrderBy(k => k.TenKhach), "Id", "TenKhach", thue.MaKhach);
            return View(thue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TraBang(int id)
        {
            var thue = await _context.Thues.FirstOrDefaultAsync(t => t.Id == id);
            if (thue == null) return NotFound();

            if (thue.NgayTra == null)
            {
                thue.NgayTra = DateTime.Today;
                thue.DaTraTienThue = true;
                TinhTien(thue);

                var bang = await _context.Bangs.FindAsync(thue.MaBang);
                if (bang != null)
                {
                    bang.TinhTrang = "Có sẵn";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Trả băng thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GiaHan(int id, int soNgay = 3)
        {
            var thue = await _context.Thues.FirstOrDefaultAsync(t => t.Id == id);
            if (thue == null) return NotFound();

            if (thue.NgayTra != null)
            {
                TempData["ErrorMessage"] = "Phiếu đã trả, không thể gia hạn.";
                return RedirectToAction(nameof(Index));
            }

            var defaultDays = _rentalSettings.SoNgayHanMacDinh <= 0 ? 3 : _rentalSettings.SoNgayHanMacDinh;
            var days = soNgay <= 0 ? defaultDays : soNgay;

            thue.HanTra = thue.HanTra.AddDays(days);
            TinhTien(thue);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Gia hạn phiếu thuê thêm {days} ngày thành công.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var thue = await _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thue == null) return NotFound();
            return View(thue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thue = await _context.Thues.FindAsync(id);
            if (thue != null)
            {
                var bang = await _context.Bangs.FindAsync(thue.MaBang);
                if (bang != null)
                {
                    bang.TinhTrang = "Có sẵn";
                }

                _context.Thues.Remove(thue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa phiếu thuê thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BaoCao(string period = "month")
        {
            var today = DateTime.Today;
            var fromDate = period switch
            {
                "week" => today.AddDays(-7),
                "quarter" => today.AddMonths(-3),
                "year" => today.AddYears(-1),
                _ => new DateTime(today.Year, today.Month, 1)
            };

            var kyLabel = period switch
            {
                "week" => "7 ngày gần nhất",
                "quarter" => "3 tháng gần nhất",
                "year" => "12 tháng gần nhất",
                _ => "tháng này"
            };

            ViewBag.Period = period;
            ViewBag.KyLabel = kyLabel;

            ViewBag.TongPhim = await _context.Phims.CountAsync();
            ViewBag.TongBang = await _context.Bangs.CountAsync();
            ViewBag.DangThue = await _context.Thues.CountAsync(t => t.NgayTra == null);
            ViewBag.QuaHan = await _context.Thues.CountAsync(t => t.NgayTra == null && t.HanTra < today);

            ViewBag.TongDoanhThu = (await _context.Thues
                .Where(t => t.DaTraTienThue)
                .Select(t => (decimal?)t.TongTien)
                .SumAsync()) ?? 0m;

            ViewBag.DoanhThuKy = (await _context.Thues
                .Where(t => t.DaTraTienThue && t.NgayThue >= fromDate)
                .Select(t => (decimal?)t.TongTien)
                .SumAsync()) ?? 0m;

            ViewBag.TopPhim = await _context.Thues
                .Include(t => t.MaBangNavigation)
                .ThenInclude(b => b!.Phim)
                .Where(t => t.NgayThue >= fromDate)
                .GroupBy(t => t.MaBangNavigation!.Phim!.TenPhim)
                .Select(g => new { Ten = g.Key, SoLan = g.Count() })
                .OrderByDescending(x => x.SoLan)
                .Take(5)
                .ToListAsync();

            ViewBag.TopKhach = await _context.Thues
                .Include(t => t.MaKhachNavigation)
                .Where(t => t.NgayThue >= fromDate)
                .GroupBy(t => t.MaKhachNavigation!.TenKhach)
                .Select(g => new { Ten = g.Key, SoLan = g.Count() })
                .OrderByDescending(x => x.SoLan)
                .Take(5)
                .ToListAsync();

            ViewBag.PhieuQuaHan = await _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .Where(t => t.NgayTra == null && t.HanTra < today)
                .OrderBy(t => t.HanTra)
                .Take(5)
                .ToListAsync();

            var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
            var revenueByMonth = await _context.Thues
                .Where(t => t.DaTraTienThue && t.NgayThue >= startMonth)
                .GroupBy(t => new { t.NgayThue.Year, t.NgayThue.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Value = g.Sum(x => x.TongTien) })
                .ToListAsync();

            var monthLabels = Enumerable.Range(0, 6)
                .Select(i => startMonth.AddMonths(i))
                .Select(d => d.ToString("MM/yyyy"))
                .ToList();

            var monthValues = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var month = startMonth.AddMonths(i);
                    return revenueByMonth
                        .Where(x => x.Year == month.Year && x.Month == month.Month)
                        .Select(x => x.Value)
                        .FirstOrDefault();
                });
            ViewBag.RevenueLabelsJson = JsonSerializer.Serialize(monthLabels);
            ViewBag.RevenueValuesJson = JsonSerializer.Serialize(monthValues);

            var statusLabels = new[] { "Có sẵn", "Đang thuê", "Hỏng", "Mất" };
            var statusValues = new[]
            {
                await _context.Bangs.CountAsync(b => b.TinhTrang == "Có sẵn"),
                await _context.Bangs.CountAsync(b => b.TinhTrang == "Đang thuê"),
                await _context.Bangs.CountAsync(b => b.TinhTrang == "Hỏng"),
                await _context.Bangs.CountAsync(b => b.TinhTrang == "Mất")
            };

            ViewBag.StatusLabelsJson = JsonSerializer.Serialize(statusLabels);
            ViewBag.StatusValuesJson = JsonSerializer.Serialize(statusValues);

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> QuaHan()
        {
            var model = await _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .Where(t => t.NgayTra == null && t.HanTra < DateTime.Today)
                .OrderBy(t => t.HanTra)
                .ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv(string? searchString, string? statusFilter, string? sortOrder)
        {
            var query = _context.Thues
                .Include(t => t.MaBangNavigation)
                .Include(t => t.MaKhachNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(t =>
                    (t.MaBangNavigation != null && t.MaBangNavigation.TenBang.Contains(searchString)) ||
                    (t.MaKhachNavigation != null && t.MaKhachNavigation.TenKhach.Contains(searchString)));
            }

            query = statusFilter switch
            {
                "dangthue" => query.Where(t => t.NgayTra == null && t.HanTra >= DateTime.Today),
                "saptoihan" => query.Where(t => t.NgayTra == null && t.HanTra >= DateTime.Today && EF.Functions.DateDiffDay(DateTime.Today, t.HanTra) <= 1),
                "quahan" => query.Where(t => t.NgayTra == null && t.HanTra < DateTime.Today),
                "datra" => query.Where(t => t.NgayTra != null),
                _ => query
            };

            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(t => t.NgayThue),
                "date_desc" => query.OrderByDescending(t => t.NgayThue),
                "remain_asc" => query.OrderBy(t => t.HanTra),
                "remain_desc" => query.OrderByDescending(t => t.HanTra),
                "fee_asc" => query.OrderBy(t => t.PhiTreHan),
                "fee_desc" => query.OrderByDescending(t => t.PhiTreHan),
                "total_asc" => query.OrderBy(t => t.TongTien),
                "total_desc" => query.OrderByDescending(t => t.TongTien),
                _ => query.OrderByDescending(t => t.NgayThue)
            };

            var items = await query.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Băng,Khách,Ngày Thuê,Hạn Trả,Ngày Trả,Phí Trễ Hạn,Tổng Tiền,Trạng Thái");

            foreach (var item in items)
            {
                var status = item.NgayTra != null ? "Đã trả" : (item.HanTra < DateTime.Today ? "Quá hạn" : "Đang thuê");
                var bangName = item.MaBangNavigation?.TenBang ?? "";
                var khachName = item.MaKhachNavigation?.TenKhach ?? "";
                var ngayTra = item.NgayTra?.ToString("dd/MM/yyyy") ?? "";

                csv.AppendLine($"\"{EscapeCsv(bangName)}\",\"{EscapeCsv(khachName)}\",\"{item.NgayThue:dd/MM/yyyy}\",\"{item.HanTra:dd/MM/yyyy}\",\"{ngayTra}\",\"{item.PhiTreHan}\",\"{item.TongTien}\",\"{EscapeCsv(status)}\"");
            }

            var fileName = $"Thues_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var bytes = encoding.GetBytes(csv.ToString());
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return input.Replace("\"", "\"\"");
        }

        private async Task PopulateCreateDropdownsAsync(Thue thue, bool isAdmin)
        {
            var availableBangs = await _context.Bangs
                .Where(b => (b.TinhTrang != "Đang thuê" && !_context.Thues.Any(t => t.MaBang == b.Id && t.NgayTra == null)) || b.Id == thue.MaBang)
                .OrderBy(b => b.TenBang)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            ViewBag.MaBang = new SelectList(availableBangs, "Id", "TenBang", thue.MaBang);

            if (isAdmin)
            {
                ViewBag.MaKhach = new SelectList(_context.Khachs.OrderBy(k => k.TenKhach), "Id", "TenKhach", thue.MaKhach);
            }
        }

        private async Task<Khach?> GetCurrentKhachAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _context.Khachs.FirstOrDefaultAsync(k => k.IdentityUserId == userId);
        }

        private void TinhTien(Thue thue)
        {
            var donGiaNgay = _rentalSettings.DonGiaNgay <= 0 ? 10000m : _rentalSettings.DonGiaNgay;
            var phiTreHanMoiNgay = _rentalSettings.PhiTreHanMoiNgay <= 0 ? 5000m : _rentalSettings.PhiTreHanMoiNgay;

            var hanTra = thue.HanTra.Date;
            var ngayThue = thue.NgayThue.Date;

            if (hanTra < ngayThue)
            {
                thue.HanTra = ngayThue;
                hanTra = ngayThue;
            }

            var soNgayDuKien = Math.Max(1, (hanTra - ngayThue).Days + 1);
            var phiCoBan = soNgayDuKien * donGiaNgay;

            var ngayTinhTre = (thue.NgayTra ?? DateTime.Today).Date;
            var soNgayTre = Math.Max(0, (ngayTinhTre - hanTra).Days);
            thue.PhiTreHan = soNgayTre * phiTreHanMoiNgay;
            thue.TongTien = phiCoBan + thue.PhiTreHan;
        }

        private async Task ValidateThueAsync(Thue thue)
        {
            if (thue.HanTra.Date < thue.NgayThue.Date)
            {
                ModelState.AddModelError(nameof(Thue.HanTra), "Hạn trả phải lớn hơn hoặc bằng ngày thuê.");
            }

            if (thue.NgayTra.HasValue && thue.NgayTra.Value.Date < thue.NgayThue.Date)
            {
                ModelState.AddModelError(nameof(Thue.NgayTra), "Ngày trả phải lớn hơn hoặc bằng ngày thuê.");
            }

            var bangDangDuocThue = await _context.Thues.AnyAsync(t =>
                t.MaBang == thue.MaBang &&
                t.NgayTra == null &&
                t.Id != thue.Id);

            if (bangDangDuocThue)
            {
                ModelState.AddModelError(nameof(Thue.MaBang), "Băng này đang được thuê, không thể tạo phiếu thuê mới.");
            }
        }
    }
}
