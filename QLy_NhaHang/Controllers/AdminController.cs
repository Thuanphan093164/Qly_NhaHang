using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Data;
using QLy_NhaHang.Models;
using QLy_NhaHang.Models.Enums;
using QLy_NhaHang.ViewModels;
using System.Text.Json;

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
        /// Danh sách món ăn (Quản lý thực đơn)
        /// </summary>
        public async Task<IActionResult> MenuItems()
        {
            try
            {
                var menuItems = await _context.MenuItems
                    .Include(m => m.Category)
                    .OrderBy(m => m.Category != null ? m.Category.Name : "")
                    .ThenBy(m => m.Name)
                    .ToListAsync();
                
                return View(menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách món ăn");
                return View(new List<MenuItem>());
            }
        }

        /// <summary>
        /// Tạo mới món ăn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateMenuItem()
        {
            await LoadCategoriesForViewBag();
            return View();
        }

        /// <summary>
        /// Xử lý tạo mới món ăn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItem menuItem)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesForViewBag();
                return View(menuItem);
            }

            try
            {
                // Kiểm tra category có tồn tại không
                var category = await _context.Categories.FindAsync(menuItem.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError(nameof(menuItem.CategoryId), "Danh mục không tồn tại");
                    await LoadCategoriesForViewBag();
                    return View(menuItem);
                }

                menuItem.IsActive = true;
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm món ăn '{menuItem.Name}' thành công!";
                _logger.LogInformation("Đã tạo món ăn mới: {MenuItemName}", menuItem.Name);
                
                return RedirectToAction(nameof(MenuItems));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo món ăn");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo món ăn. Vui lòng thử lại.");
                await LoadCategoriesForViewBag();
                return View(menuItem);
            }
        }

        /// <summary>
        /// Chỉnh sửa món ăn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (menuItem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món ăn này!";
                    return RedirectToAction(nameof(MenuItems));
                }

                await LoadCategoriesForViewBag();
                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin món ăn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin món ăn.";
                return RedirectToAction(nameof(MenuItems));
            }
        }

        /// <summary>
        /// Xử lý cập nhật món ăn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(int id, MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(MenuItems));
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesForViewBag();
                return View(menuItem);
            }

            try
            {
                // Kiểm tra category có tồn tại không
                var category = await _context.Categories.FindAsync(menuItem.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError(nameof(menuItem.CategoryId), "Danh mục không tồn tại");
                    await LoadCategoriesForViewBag();
                    return View(menuItem);
                }

                _context.Update(menuItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật món ăn '{menuItem.Name}' thành công!";
                _logger.LogInformation("Đã cập nhật món ăn: {MenuItemName}", menuItem.Name);
                
                return RedirectToAction(nameof(MenuItems));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MenuItemExists(menuItem.Id))
                {
                    TempData["ErrorMessage"] = "Món ăn này không còn tồn tại!";
                    return RedirectToAction(nameof(MenuItems));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật món ăn");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật món ăn. Vui lòng thử lại.");
                await LoadCategoriesForViewBag();
                return View(menuItem);
            }
        }

        /// <summary>
        /// Xóa món ăn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems.FindAsync(id);
                if (menuItem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món ăn này!";
                    return RedirectToAction(nameof(MenuItems));
                }

                // Kiểm tra xem món ăn có đang được sử dụng trong Orders không
                var hasOrderDetails = await _context.OrderDetails.AnyAsync(od => od.MenuItemId == id);
                if (hasOrderDetails)
                {
                    // Thay vì xóa, đánh dấu là không hoạt động
                    menuItem.IsActive = false;
                    _context.MenuItems.Update(menuItem);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Đã tạm ngưng món ăn '{menuItem.Name}' (có đơn hàng liên quan)!";
                }
                else
                {
                    _context.MenuItems.Remove(menuItem);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Đã xóa món ăn '{menuItem.Name}' thành công!";
                }

                _logger.LogInformation("Đã xóa/tạm ngưng món ăn: {MenuItemName}", menuItem.Name);
                
                return RedirectToAction(nameof(MenuItems));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa món ăn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa món ăn. Vui lòng thử lại.";
                return RedirectToAction(nameof(MenuItems));
            }
        }

        /// <summary>
        /// Kiểm tra món ăn có tồn tại không
        /// </summary>
        private async Task<bool> MenuItemExists(int id)
        {
            return await _context.MenuItems.AnyAsync(e => e.Id == id);
        }

        /// <summary>
        /// Load danh sách categories cho ViewBag
        /// </summary>
        private async Task LoadCategoriesForViewBag()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
        }

        /// <summary>
        /// Tạo mới Món ăn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateMonAn()
        {
            ViewBag.CategoryName = "Món ăn";
            ViewBag.CategoryId = await GetCategoryIdByNameAsync("Món ăn");
            ViewBag.ActionName = "CreateMonAn";
            return View("CreateMenuItem");
        }

        /// <summary>
        /// Xử lý tạo mới Món ăn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMonAn(MenuItem menuItem)
        {
            var categoryId = await GetCategoryIdByNameAsync("Món ăn");
            if (categoryId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục 'Món ăn'. Vui lòng tạo danh mục trước.";
                return RedirectToAction(nameof(MenuItems));
            }

            menuItem.CategoryId = categoryId.Value;
            return await CreateMenuItemInternal(menuItem, "Món ăn");
        }

        /// <summary>
        /// Tạo mới Thức uống
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateThucUong()
        {
            ViewBag.CategoryName = "Thức uống";
            ViewBag.CategoryId = await GetCategoryIdByNameAsync("Thức uống");
            ViewBag.ActionName = "CreateThucUong";
            return View("CreateMenuItem");
        }

        /// <summary>
        /// Xử lý tạo mới Thức uống
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThucUong(MenuItem menuItem)
        {
            var categoryId = await GetCategoryIdByNameAsync("Thức uống");
            if (categoryId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục 'Thức uống'. Vui lòng tạo danh mục trước.";
                return RedirectToAction(nameof(MenuItems));
            }

            menuItem.CategoryId = categoryId.Value;
            return await CreateMenuItemInternal(menuItem, "Thức uống");
        }

        /// <summary>
        /// Tạo mới Rượu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateRuou()
        {
            ViewBag.CategoryName = "Rượu";
            ViewBag.CategoryId = await GetCategoryIdByNameAsync("Rượu");
            ViewBag.ActionName = "CreateRuou";
            return View("CreateMenuItem");
        }

        /// <summary>
        /// Xử lý tạo mới Rượu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRuou(MenuItem menuItem)
        {
            var categoryId = await GetCategoryIdByNameAsync("Rượu");
            if (categoryId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục 'Rượu'. Vui lòng tạo danh mục trước.";
                return RedirectToAction(nameof(MenuItems));
            }

            menuItem.CategoryId = categoryId.Value;
            return await CreateMenuItemInternal(menuItem, "Rượu");
        }

        /// <summary>
        /// Helper method để tạo menu item
        /// </summary>
        private async Task<IActionResult> CreateMenuItemInternal(MenuItem menuItem, string categoryName)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryName = categoryName;
                ViewBag.CategoryId = menuItem.CategoryId;
                ViewBag.ActionName = categoryName switch
                {
                    "Món ăn" => "CreateMonAn",
                    "Thức uống" => "CreateThucUong",
                    "Rượu" => "CreateRuou",
                    _ => "CreateMenuItem"
                };
                return View("CreateMenuItem", menuItem);
            }

            try
            {
                menuItem.IsActive = true;
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm {categoryName.ToLower()} '{menuItem.Name}' thành công!";
                _logger.LogInformation("Đã tạo {CategoryName} mới: {MenuItemName}", categoryName, menuItem.Name);
                
                return RedirectToAction(nameof(MenuItems));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo {CategoryName}", categoryName);
                ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo {categoryName.ToLower()}. Vui lòng thử lại.");
                ViewBag.CategoryName = categoryName;
                ViewBag.CategoryId = menuItem.CategoryId;
                ViewBag.ActionName = categoryName switch
                {
                    "Món ăn" => "CreateMonAn",
                    "Thức uống" => "CreateThucUong",
                    "Rượu" => "CreateRuou",
                    _ => "CreateMenuItem"
                };
                return View("CreateMenuItem", menuItem);
            }
        }

        /// <summary>
        /// Lấy CategoryId theo tên
        /// </summary>
        private async Task<int?> GetCategoryIdByNameAsync(string categoryName)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName && c.IsActive == true);
            return category?.Id;
        }

        /// <summary>
        /// AJAX: Lấy danh sách menu items theo category
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenuItemsList(string type)
        {
            try
            {
                string categoryName = type switch
                {
                    "mon-an" => "Món ăn",
                    "thuc-uong" => "Thức uống",
                    "ruou" => "Rượu",
                    _ => null
                };

                var query = _context.MenuItems
                    .Include(m => m.Category)
                    .AsQueryable();

                if (categoryName != null)
                {
                    query = query.Where(m => m.Category != null && m.Category.Name == categoryName);
                }

                var menuItems = await query
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                ViewBag.CategoryName = categoryName ?? "Tất cả";
                ViewBag.Type = type;

                return PartialView("_MenuItemsList", menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách menu items cho type: {Type}", type);
                return Content($"<div class='alert alert-danger'>Có lỗi xảy ra: {ex.Message}</div>", "text/html");
            }
        }

        /// <summary>
        /// AJAX: Lấy form đăng bài theo loại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCreateForm(string type)
        {
            try
            {
                string categoryName = type switch
                {
                    "mon-an" => "Món ăn",
                    "thuc-uong" => "Thức uống",
                    "ruou" => "Rượu",
                    _ => null
                };

                if (categoryName == null)
                {
                    return Content("<div class='alert alert-danger'>Loại không hợp lệ. Vui lòng chọn: mon-an, thuc-uong, hoặc ruou.</div>", "text/html");
                }

                var categoryId = await GetCategoryIdByNameAsync(categoryName);
                if (categoryId == null)
                {
                    // Tự động tạo category nếu chưa có
                    var newCategory = new Category
                    {
                        Name = categoryName,
                        IsActive = true
                    };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    categoryId = newCategory.Id;
                    _logger.LogInformation("Đã tự động tạo category: {CategoryName}", categoryName);
                }

                ViewBag.CategoryName = categoryName;
                ViewBag.CategoryId = categoryId;
                ViewBag.ActionName = type switch
                {
                    "mon-an" => "CreateMonAn",
                    "thuc-uong" => "CreateThucUong",
                    "ruou" => "CreateRuou",
                    _ => "CreateMenuItem"
                };

                var menuItem = new MenuItem
                {
                    CategoryId = categoryId.Value,
                    Unit = "phần",
                    IsActive = true
                };

                return PartialView("_CreateMenuItemForm", menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy form đăng bài cho type: {Type}", type);
                return Content($"<div class='alert alert-danger'>Có lỗi xảy ra: {ex.Message}</div>", "text/html");
            }
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
        /// Danh sách đơn hàng real-time cho bếp/bar - Hiển thị các bàn có đơn hàng
        /// </summary>
        public async Task<IActionResult> Orders()
        {
            try
            {
                // Lấy danh sách bàn có đơn hàng đang xử lý (New, Processing) - không lấy Served và Paid
                var orders = await _context.Orders
                    .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Processing)
                    .Include(o => o.Table)
                    .ToListAsync();

                // Group theo TableId và tính toán
                var tablesWithOrders = orders
                    .GroupBy(o => o.TableId)
                    .Select(g => new
                    {
                        TableId = g.Key,
                        Table = g.First().Table,
                        Orders = g.OrderBy(o => o.OrderDate).ToList(), // Sắp xếp theo thời gian đặt (cũ nhất trước)
                        OldestOrderDate = g.Min(o => o.OrderDate), // Lấy đơn cũ nhất để ưu tiên
                        LatestOrderDate = g.Max(o => o.OrderDate),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.OldestOrderDate) // Sắp xếp: bàn nào đặt trước (cũ hơn) lên đầu
                    .ThenBy(x => x.Table?.Name) // Nếu cùng thời gian, sắp xếp theo tên bàn
                    .ToList();

                // Tạo ViewModel
                var viewModel = tablesWithOrders.Select(x => new TableOrderViewModel
                {
                    TableId = x.TableId,
                    TableName = x.Table?.Name ?? "Không xác định",
                    OrderCount = x.Orders.Count,
                    LatestOrderDate = x.LatestOrderDate,
                    TotalAmount = x.TotalAmount,
                    Status = (int)(x.Table?.CurrentStatus ?? TableStatus.Free)
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách đơn hàng");
                return View(new List<TableOrderViewModel>());
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng của bàn (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int tableId)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.TableId == tableId && (o.Status == OrderStatus.New || o.Status == OrderStatus.Processing))
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .OrderBy(o => o.OrderDate) // Sắp xếp: đơn cũ nhất trước
                    .ToListAsync();

                if (!orders.Any())
                {
                    var emptyViewModel = new OrderDetailsViewModel
                    {
                        TableId = tableId,
                        TableName = "Không xác định",
                        Orders = new List<OrderInfo>()
                    };
                    return PartialView("_OrderDetails", emptyViewModel);
                }

                var table = orders.First().Table;
                var viewModel = new OrderDetailsViewModel
                {
                    TableId = tableId,
                    TableName = table?.Name ?? "Không xác định",
                    Orders = orders.Select(o => new OrderInfo
                    {
                        OrderId = o.Id,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = (int)o.Status,
                        OrderDetails = o.OrderDetails.Select(od => 
                        {
                            // Kiểm tra trạng thái từ session
                            var key = $"OrderDetail_{od.Id}_Completed";
                            var isCompletedStr = HttpContext.Session.GetString(key);
                            var isCompleted = !string.IsNullOrEmpty(isCompletedStr) && bool.Parse(isCompletedStr);
                            
                            return new OrderDetailInfo
                            {
                                OrderDetailId = od.Id,
                                MenuItemName = od.MenuItem?.Name ?? "Không xác định",
                                Quantity = od.Quantity,
                                Price = od.Price,
                                SubTotal = od.Quantity * od.Price,
                                IsCompleted = isCompleted
                            };
                        }).ToList()
                    }).ToList()
                };

                return PartialView("_OrderDetails", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết đơn hàng cho bàn {TableId}", tableId);
                var errorViewModel = new OrderDetailsViewModel
                {
                    TableId = tableId,
                    TableName = "Không xác định",
                    Orders = new List<OrderInfo>()
                };
                return PartialView("_OrderDetails", errorViewModel);
            }
        }

        /// <summary>
        /// Đánh dấu món đã hoàn thành (AJAX)
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken] // Bỏ qua AntiForgeryToken cho AJAX request
        public async Task<IActionResult> MarkOrderDetailComplete([FromBody] MarkOrderDetailCompleteRequest request)
        {
            try
            {
                if (request == null || request.OrderDetailId <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Tạm thời lưu vào session, sau này sẽ thêm field vào database
                var key = $"OrderDetail_{request.OrderDetailId}_Completed";
                HttpContext.Session.SetString(key, request.IsCompleted.ToString());

                _logger.LogInformation("Đã đánh dấu OrderDetail {OrderDetailId} là {IsCompleted}", 
                    request.OrderDetailId, request.IsCompleted ? "đã hoàn thành" : "chưa hoàn thành");

                return Json(new { success = true, message = "Đã cập nhật trạng thái món!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu món đã hoàn thành");
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng thành "Đã hoàn thành" và xóa khỏi danh sách
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CompleteOrder([FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);
                
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                // Cập nhật trạng thái đơn hàng thành Served (Đã phục vụ)
                order.Status = OrderStatus.Served;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã hoàn thành đơn hàng {OrderId}", request.OrderId);

                return Json(new { 
                    success = true, 
                    message = "Đã hoàn thành đơn hàng!",
                    reload = true // Signal để reload page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành đơn hàng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi hoàn thành đơn hàng. Vui lòng thử lại." });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (cho bếp/bar) - Deprecated, dùng CompleteOrder thay thế
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                var newStatus = (OrderStatus)request.Status;
                if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ!" });
                }

                order.Status = newStatus;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật trạng thái đơn hàng {OrderId} thành {Status}", request.OrderId, newStatus);

                return Json(new { success = true, message = "Đã cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái. Vui lòng thử lại." });
            }
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

