using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Data;
using QLy_NhaHang.Models;
using QLy_NhaHang.Models.Enums;
using QLy_NhaHang.ViewModels;

namespace QLy_NhaHang.Controllers
{
    /// <summary>
    /// Controller xử lý đặt bàn từ xa (Remote User)
    /// </summary>
    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> _logger;
        private readonly QlyNhaHangContext _context;

        public BookingController(ILogger<BookingController> logger, QlyNhaHangContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Trang chọn bàn - Hiển thị danh sách bàn với filter
        /// </summary>
        public async Task<IActionResult> Index(string filter = "all")
        {
            try
            {
                var tables = await GetTablesWithAvailabilityAsync();
                
                // Lọc bàn theo trạng thái
                if (!string.IsNullOrEmpty(filter) && filter != "all")
                {
                    var statusFilter = filter switch
                    {
                        "free" => TableStatus.Free,
                        "occupied" => TableStatus.Occupied,
                        "reserved" => TableStatus.Reserved,
                        _ => (TableStatus?)null
                    };
                    
                    if (statusFilter.HasValue)
                    {
                        tables = tables.Where(t => t.TrangThai == statusFilter.Value).ToList();
                    }
                }
                
                // Sắp xếp bàn theo số tăng dần (1, 2, 3, 4, 5, 6...)
                tables = tables.OrderBy(t => ExtractNumberFromTableName(t.Ten)).ToList();
                
                var viewModel = new TableSelectionViewModel
                {
                    Tables = tables,
                    SelectedDateTime = DateTime.Now.AddHours(1),
                    CurrentFilter = filter
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách bàn");
                return View(new TableSelectionViewModel());
            }
        }

        /// <summary>
        /// Trích xuất số từ tên bàn (ví dụ: "Bàn 01" -> 1, "Bàn 12" -> 12)
        /// </summary>
        private int ExtractNumberFromTableName(string tableName)
        {
            // Tìm tất cả các số trong tên bàn
            var numbers = System.Text.RegularExpressions.Regex.Match(tableName, @"\d+");
            if (numbers.Success && int.TryParse(numbers.Value, out int number))
            {
                return number;
            }
            // Nếu không tìm thấy số, trả về 0 để đặt ở cuối
            return 0;
        }

        /// <summary>
        /// Lấy danh sách bàn với trạng thái
        /// </summary>
        private async Task<List<TableViewModel>> GetTablesWithAvailabilityAsync()
        {
            var tables = await _context.Tables
                .Where(t => !t.IsHidden)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return tables.Select(t => new TableViewModel
            {
                Ma = t.Id,
                Ten = t.Name,
                SucChua = t.Capacity,
                TrangThai = t.CurrentStatus,
                IsAvailable = t.CurrentStatus == TableStatus.Free // Chỉ bàn Free mới có thể đặt
            }).ToList();
        }

        /// <summary>
        /// Hiển thị form đặt bàn (GET) - Có thể có tableId từ query string
        /// </summary>
        [HttpGet]
        public IActionResult Create(int? tableId = null)
        {
            var viewModel = new BookingCreateViewModel
            {
                MaBan = tableId,
                GioDat = DateTime.Now.AddHours(1) // Mặc định là 1 giờ sau
            };
            return View(viewModel);
        }

        /// <summary>
        /// Xử lý submit form đặt bàn (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel viewModel)
        {
            // Kiểm tra validation cơ bản
            if (!ModelState.IsValid)
            {
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(" ", errors) });
                }
                return View(viewModel);
            }

            // Kiểm tra logic: Giờ đặt phải > giờ hiện tại
            if (viewModel.GioDat <= DateTime.Now)
            {
                var errorMessage = "Thời gian đặt bàn phải sau thời điểm hiện tại";
                ModelState.AddModelError(nameof(viewModel.GioDat), errorMessage);
                
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                return View(viewModel);
            }

            try
            {
                // Map từ ViewModel sang Model
                var booking = MapToBooking(viewModel);

                // Nếu có chọn bàn, cập nhật trạng thái bàn thành Reserved
                if (viewModel.MaBan.HasValue)
                {
                    var table = await _context.Tables.FindAsync(viewModel.MaBan.Value);
                    if (table != null)
                    {
                        // Kiểm tra bàn có đang trống không
                        if (table.CurrentStatus != TableStatus.Free)
                        {
                            var errorMessage = $"Bàn {table.Name} không còn trống. Vui lòng chọn bàn khác.";
                            ModelState.AddModelError("", errorMessage);
                            
                            // Nếu là AJAX request, trả về JSON
                            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                return Json(new { success = false, message = errorMessage });
                            }
                            return View(viewModel);
                        }

                        // Cập nhật trạng thái bàn thành Reserved
                        table.CurrentStatus = TableStatus.Reserved;
                        _context.Tables.Update(table);
                        _logger.LogInformation("Đã cập nhật trạng thái bàn {TableName} thành Reserved", table.Name);
                    }
                    else
                    {
                        var errorMessage = "Không tìm thấy bàn được chọn. Vui lòng thử lại.";
                        ModelState.AddModelError("", errorMessage);
                        
                        // Nếu là AJAX request, trả về JSON
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = errorMessage });
                        }
                        return View(viewModel);
                    }
                }

                // Lưu vào database
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đặt bàn thành công: {CustomerName} - {PhoneNumber} - {BookingTime}", 
                    booking.CustomerName, booking.PhoneNumber, booking.BookingTime);

                var successMessage = $"Đặt bàn thành công! Chúng tôi sẽ liên hệ với bạn qua số điện thoại {viewModel.SoDienThoai} để xác nhận.";
                
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = successMessage });
                }

                // Redirect với thông báo thành công (cho non-AJAX requests)
                TempData["SuccessMessage"] = successMessage;
                return RedirectToAction(nameof(Success));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đặt bàn");
                var errorMessage = "Có lỗi xảy ra khi đặt bàn. Vui lòng thử lại sau.";
                ModelState.AddModelError("", errorMessage);
                
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                return View(viewModel);
            }
        }

        /// <summary>
        /// Trang thông báo đặt bàn thành công
        /// </summary>
        public IActionResult Success()
        {
            return View();
        }

        /// <summary>
        /// Kiểm tra bàn trống trong khung giờ được chọn (sẽ implement sau)
        /// </summary>
        [HttpPost]
        public Task<IActionResult> CheckAvailability(DateTime gioDat, int soNguoi)
        {
            try
            {
                // TODO: Logic kiểm tra bàn trống sẽ được implement sau
                // Tạm thời trả về success
                return Task.FromResult<IActionResult>(Json(new { success = true, message = "Có bàn trống", availableTables = new List<int>() }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra bàn trống");
                return Task.FromResult<IActionResult>(Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra bàn trống" }));
            }
        }

        /// <summary>
        /// Map từ ViewModel sang Model
        /// </summary>
        private Booking MapToBooking(BookingCreateViewModel viewModel)
        {
            return new Booking
            {
                CustomerName = viewModel.TenKhach,
                PhoneNumber = viewModel.SoDienThoai,
                BookingTime = viewModel.GioDat,
                GuestCount = viewModel.SoNguoi,
                Note = viewModel.GhiChu,
                Status = BookingStatus.Pending, // Trạng thái mới đặt = 0
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// AJAX: Lấy menu items theo category để hiển thị trong modal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenuItemsByCategory(string category)
        {
            try
            {
                var categoryName = category?.ToLower() switch
                {
                    "mon-an" => "Món ăn",
                    "thuc-uong" => "Thức uống",
                    "ruou" => "Rượu",
                    _ => null
                };

                var query = _context.MenuItems
                    .Where(m => m.IsActive == true)
                    .Include(m => m.Category)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(categoryName))
                {
                    query = query.Where(m => m.Category != null && m.Category.Name == categoryName);
                }

                var menuItems = await query
                    .OrderBy(m => m.Category != null ? m.Category.Name : "")
                    .ThenBy(m => m.Name)
                    .ToListAsync();

                var menuItemViewModels = menuItems.Select(item => new MenuItemViewModel
                {
                    Id = item.Id,
                    Ten = item.Name,
                    MoTa = item.Description,
                    Gia = item.Price,
                    Anh = item.ImageUrl,
                    DonVi = item.Unit,
                    TenDanhMuc = item.Category?.Name
                }).ToList();

                ViewBag.CategoryName = categoryName ?? "Tất cả";
                return PartialView("_MenuItemsModal", menuItemViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải menu items theo category");
                ViewBag.CategoryName = "Tất cả";
                return PartialView("_MenuItemsModal", new List<MenuItemViewModel>());
            }
        }

        /// <summary>
        /// Xử lý đặt món từ khách hàng tại bàn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int tableId, string items)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (tableId <= 0 || string.IsNullOrEmpty(items))
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Parse items từ JSON string
                List<OrderItemRequest>? orderItems = null;
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Cho phép camelCase và PascalCase
                    };
                    orderItems = System.Text.Json.JsonSerializer.Deserialize<List<OrderItemRequest>>(items, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi parse JSON items: {Items}", items);
                    return Json(new { success = false, message = "Dữ liệu món không hợp lệ!" });
                }
                
                if (orderItems == null || !orderItems.Any())
                {
                    return Json(new { success = false, message = "Danh sách món không hợp lệ!" });
                }

                // Validate từng item
                foreach (var item in orderItems)
                {
                    if (item.MenuItemId <= 0)
                    {
                        _logger.LogWarning("MenuItemId không hợp lệ: {MenuItemId}", item.MenuItemId);
                        return Json(new { success = false, message = $"ID món không hợp lệ: {item.MenuItemId}" });
                    }
                    if (item.Quantity <= 0)
                    {
                        return Json(new { success = false, message = $"Số lượng món không hợp lệ: {item.Quantity}" });
                    }
                }

                // Kiểm tra bàn có tồn tại không
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn này!" });
                }

                // Tính tổng tiền và validate menu items
                decimal totalAmount = 0;
                foreach (var item in orderItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem == null)
                    {
                        _logger.LogWarning("Không tìm thấy món với ID: {MenuItemId}", item.MenuItemId);
                        return Json(new { success = false, message = $"Không tìm thấy món với ID: {item.MenuItemId}" });
                    }
                    totalAmount += menuItem.Price * item.Quantity;
                }

                // Tạo đơn hàng
                var order = new Order
                {
                    TableId = tableId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.New
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy OrderId

                // Tạo chi tiết đơn hàng
                foreach (var item in orderItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.Id,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            Price = menuItem.Price
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }
                }

                // Cập nhật trạng thái bàn
                // Nếu bàn đang là Free hoặc Reserved, chuyển sang Occupied
                // Nếu bàn đã là Occupied, giữ nguyên
                var oldStatus = table.CurrentStatus;
                if (table.CurrentStatus == TableStatus.Free || table.CurrentStatus == TableStatus.Reserved)
                {
                    table.CurrentStatus = TableStatus.Occupied;
                    _context.Tables.Update(table);
                    _logger.LogInformation("Đã cập nhật trạng thái bàn {TableName} từ {OldStatus} sang Occupied", 
                        table.Name, oldStatus);
                }
                else
                {
                    _logger.LogInformation("Bàn {TableName} đã có khách, giữ nguyên trạng thái Occupied", table.Name);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo đơn hàng {OrderId} cho bàn {TableName} với tổng tiền {TotalAmount}", 
                    order.Id, table.Name, totalAmount);

                return Json(new { 
                    success = true, 
                    message = "Đặt món thành công!",
                    orderId = order.Id,
                    totalAmount = totalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt món. Vui lòng thử lại." });
            }
        }
    }

    /// <summary>
    /// Request model cho món trong đơn hàng
    /// </summary>
    public class OrderItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}

