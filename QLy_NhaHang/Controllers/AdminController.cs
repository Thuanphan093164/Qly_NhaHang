using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Data;
using QLy_NhaHang.Models;
using QLy_NhaHang.Models.Enums;
using QLy_NhaHang.ViewModels;

namespace QLy_NhaHang.Controllers
{
    /// <summary>
    /// Controller dành cho quản lý (Admin/Staff)
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly QlyNhaHangContext _context;

        public AdminController(ILogger<AdminController> logger, QlyNhaHangContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Trang dashboard quản lý
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await GetDashboardStatsAsync();
                ViewBag.TotalTables = stats.TotalTables;
                ViewBag.FreeTables = stats.FreeTables;
                ViewBag.OccupiedTables = stats.OccupiedTables;
                ViewBag.ReservedTables = stats.ReservedTables;
                ViewBag.TotalGuests = stats.TotalGuests;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thống kê dashboard");
                // Trả về giá trị mặc định nếu có lỗi
                ViewBag.TotalTables = 0;
                ViewBag.FreeTables = 0;
                ViewBag.OccupiedTables = 0;
                ViewBag.ReservedTables = 0;
                ViewBag.TotalGuests = 0;
                return View();
            }
        }

        /// <summary>
        /// API endpoint để lấy thống kê real-time (JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await GetDashboardStatsAsync();
                return Json(new
                {
                    success = true,
                    totalTables = stats.TotalTables,
                    freeTables = stats.FreeTables,
                    occupiedTables = stats.OccupiedTables,
                    reservedTables = stats.ReservedTables,
                    totalGuests = stats.TotalGuests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê dashboard");
                return Json(new { success = false, message = "Lỗi khi lấy thống kê" });
            }
        }

        /// <summary>
        /// Lấy thống kê từ database
        /// </summary>
        private async Task<(int TotalTables, int FreeTables, int OccupiedTables, int ReservedTables, int TotalGuests)> GetDashboardStatsAsync()
        {
            var totalTables = await _context.Tables.CountAsync();
            var freeTables = await _context.Tables.CountAsync(t => t.CurrentStatus == TableStatus.Free);
            var occupiedTables = await _context.Tables.CountAsync(t => t.CurrentStatus == TableStatus.Occupied);
            var reservedTables = await _context.Tables.CountAsync(t => t.CurrentStatus == TableStatus.Reserved);

            // Tính tổng sức chứa của các bàn đang có khách (ước tính số khách)
            var totalGuests = await _context.Tables
                .Where(t => t.CurrentStatus == TableStatus.Occupied)
                .SumAsync(t => (int?)t.Capacity) ?? 0;

            return (totalTables, freeTables, occupiedTables, reservedTables, totalGuests);
        }

        #region Quản lý Món ăn (MenuItem)

        /// <summary>
        /// Danh sách món ăn
        /// </summary>
        public IActionResult MenuItems()
        {
            // TODO: Logic load danh sách món ăn sẽ được implement sau
            return View();
        }

        /// <summary>
        /// Tạo mới món ăn
        /// </summary>
        [HttpGet]
        public IActionResult CreateMenuItem()
        {
            return View();
        }

        /// <summary>
        /// Xử lý tạo mới món ăn
        /// </summary>
        [HttpPost]
        public IActionResult CreateMenuItem(MenuItem menuItem)
        {
            // TODO: Logic tạo món ăn sẽ được implement sau
            if (ModelState.IsValid)
            {
                return RedirectToAction("MenuItems");
            }
            return View(menuItem);
        }

        /// <summary>
        /// Chỉnh sửa món ăn
        /// </summary>
        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            // TODO: Logic load món ăn sẽ được implement sau
            return View();
        }

        /// <summary>
        /// Xử lý cập nhật món ăn
        /// </summary>
        [HttpPost]
        public IActionResult EditMenuItem(MenuItem menuItem)
        {
            // TODO: Logic cập nhật món ăn sẽ được implement sau
            if (ModelState.IsValid)
            {
                return RedirectToAction("MenuItems");
            }
            return View(menuItem);
        }

        /// <summary>
        /// Xóa món ăn
        /// </summary>
        [HttpPost]
        public IActionResult DeleteMenuItem(int id)
        {
            // TODO: Logic xóa món ăn sẽ được implement sau
            return RedirectToAction("MenuItems");
        }

        #endregion

        #region Quản lý Bàn (Table)

        /// <summary>
        /// Danh sách bàn
        /// </summary>
        public async Task<IActionResult> Tables()
        {
            try
            {
                var tables = await _context.Tables
                    .ToListAsync();
                
                // Sắp xếp theo thứ tự ưu tiên: Đã đặt (2) -> Có khách (1) -> Trống (0)
                // Sau đó sắp xếp theo tên bàn trong cùng một trạng thái
                var sortedTables = tables
                    .OrderByDescending(t => GetStatusPriority(t.CurrentStatus)) // Ưu tiên theo trạng thái
                    .ThenBy(t => t.Name)                                       // Trong cùng trạng thái, sắp xếp theo tên
                    .ToList();
                
                return View(sortedTables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách bàn");
                return View(new List<Table>());
            }
        }

        /// <summary>
        /// Tạo mới bàn
        /// </summary>
        [HttpGet]
        public IActionResult CreateTable()
        {
            ViewBag.StatusList = GetTableStatusSelectList();
            return View();
        }

        /// <summary>
        /// Xử lý tạo mới bàn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTable(Table table)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = GetTableStatusSelectList();
                return View(table);
            }

            try
            {
                // Kiểm tra tên bàn đã tồn tại chưa
                var existingTable = await _context.Tables
                    .FirstOrDefaultAsync(t => t.Name == table.Name);
                
                if (existingTable != null)
                {
                    ModelState.AddModelError(nameof(table.Name), "Tên bàn đã tồn tại");
                    ViewBag.StatusList = GetTableStatusSelectList();
                    return View(table);
                }

                _context.Tables.Add(table);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm bàn {table.Name} thành công!";
                _logger.LogInformation("Đã tạo bàn mới: {TableName}", table.Name);
                
                return RedirectToAction(nameof(Tables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bàn");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bàn. Vui lòng thử lại.");
                ViewBag.StatusList = GetTableStatusSelectList();
                return View(table);
            }
        }

        /// <summary>
        /// Chỉnh sửa bàn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditTable(int id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bàn này!";
                    return RedirectToAction(nameof(Tables));
                }

                ViewBag.StatusList = GetTableStatusSelectList();
                return View(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin bàn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin bàn.";
                return RedirectToAction(nameof(Tables));
            }
        }

        /// <summary>
        /// Xử lý cập nhật bàn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTable(int id, Table table)
        {
            if (id != table.Id)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(Tables));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = GetTableStatusSelectList();
                return View(table);
            }

            try
            {
                // Kiểm tra tên bàn đã tồn tại chưa (trừ bàn hiện tại)
                var existingTable = await _context.Tables
                    .FirstOrDefaultAsync(t => t.Name == table.Name && t.Id != id);
                
                if (existingTable != null)
                {
                    ModelState.AddModelError(nameof(table.Name), "Tên bàn đã tồn tại");
                    ViewBag.StatusList = GetTableStatusSelectList();
                    return View(table);
                }

                _context.Update(table);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật bàn {table.Name} thành công!";
                _logger.LogInformation("Đã cập nhật bàn: {TableName}", table.Name);
                
                return RedirectToAction(nameof(Tables));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TableExists(table.Id))
                {
                    TempData["ErrorMessage"] = "Bàn này không còn tồn tại!";
                    return RedirectToAction(nameof(Tables));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bàn");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật bàn. Vui lòng thử lại.");
                ViewBag.StatusList = GetTableStatusSelectList();
                return View(table);
            }
        }

        /// <summary>
        /// Xóa bàn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bàn này!";
                    return RedirectToAction(nameof(Tables));
                }

                // Kiểm tra xem bàn có đang được sử dụng trong Orders không
                var hasOrders = await _context.Orders.AnyAsync(o => o.TableId == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa bàn {table.Name} vì đang có đơn hàng liên quan!";
                    return RedirectToAction(nameof(Tables));
                }

                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa bàn {table.Name} thành công!";
                _logger.LogInformation("Đã xóa bàn: {TableName}", table.Name);
                
                return RedirectToAction(nameof(Tables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bàn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bàn. Vui lòng thử lại.";
                return RedirectToAction(nameof(Tables));
            }
        }

        /// <summary>
        /// Kiểm tra bàn có tồn tại không
        /// </summary>
        private async Task<bool> TableExists(int id)
        {
            return await _context.Tables.AnyAsync(e => e.Id == id);
        }

        /// <summary>
        /// Lấy danh sách trạng thái bàn cho SelectList
        /// </summary>
        private SelectList GetTableStatusSelectList()
        {
            var statuses = Enum.GetValues(typeof(TableStatus))
                .Cast<TableStatus>()
                .Select(s => new { Value = (int)s, Text = GetTableStatusText(s) })
                .ToList();

            return new SelectList(statuses, "Value", "Text");
        }

        /// <summary>
        /// Lấy độ ưu tiên sắp xếp cho trạng thái bàn (số càng cao càng ưu tiên)
        /// </summary>
        private int GetStatusPriority(TableStatus status)
        {
            return status switch
            {
                TableStatus.Reserved => 3,  // Bàn đã đặt - ưu tiên cao nhất
                TableStatus.Occupied => 2,  // Bàn có khách - ưu tiên thứ 2
                TableStatus.Free => 1,      // Bàn trống - ưu tiên thấp nhất
                _ => 0
            };
        }

        /// <summary>
        /// Lấy text hiển thị cho trạng thái bàn
        /// </summary>
        private string GetTableStatusText(TableStatus status)
        {
            return status switch
            {
                TableStatus.Free => "Trống",
                TableStatus.Occupied => "Có khách",
                TableStatus.Reserved => "Đã đặt trước",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Cập nhật trạng thái bàn nhanh (AJAX)
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken] // Bỏ qua AntiForgeryToken cho AJAX request
        public async Task<IActionResult> UpdateTableStatus([FromBody] UpdateTableStatusRequest request)
        {
            try
            {
                var table = await _context.Tables.FindAsync(request.TableId);
                if (table == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn này!" });
                }

                // Chuyển đổi status từ int sang enum
                var newStatus = (TableStatus)request.NewStatus;
                
                // Kiểm tra giá trị hợp lệ
                if (!Enum.IsDefined(typeof(TableStatus), newStatus))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ!" });
                }

                // Cập nhật trạng thái
                table.CurrentStatus = newStatus;
                _context.Tables.Update(table);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật trạng thái bàn {TableId} thành {Status}", request.TableId, newStatus);

                return Json(new { success = true, message = "Đã cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái bàn");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái. Vui lòng thử lại." });
            }
        }

        #endregion

        #region Quản lý Đơn hàng (Order) - Bếp/Bar

        /// <summary>
        /// Danh sách đơn hàng real-time cho bếp/bar
        /// </summary>
        public IActionResult Orders()
        {
            // TODO: Logic load đơn hàng real-time sẽ được implement sau
            return View();
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (cho bếp/bar)
        /// </summary>
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, OrderStatus status)
        {
            // TODO: Logic cập nhật trạng thái đơn sẽ được implement sau
            return Json(new { success = true, message = "Đã cập nhật trạng thái" });
        }

        #endregion

        #region Dự báo lượng khách

        /// <summary>
        /// Trang dự báo lượng khách theo khung giờ
        /// </summary>
        public IActionResult Forecast()
        {
            // TODO: Logic dự báo lượng khách sẽ được implement sau
            return View();
        }

        #endregion
    }
}

