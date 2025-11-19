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
    }
}

