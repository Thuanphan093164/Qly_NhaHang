using Microsoft.AspNetCore.Mvc;
using QLy_NhaHang.Models;

namespace QLy_NhaHang.Controllers
{
    /// <summary>
    /// Controller xử lý gọi món tại bàn (In-house User - QR Code)
    /// </summary>
    public class TableOrderController : Controller
    {
        private readonly ILogger<TableOrderController> _logger;

        public TableOrderController(ILogger<TableOrderController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Trang gọi món - hiển thị khi quét QR code (có thể kèm mã bàn trên URL)
        /// </summary>
        /// <param name="tableId">Mã bàn (từ QR code hoặc URL parameter)</param>
        public IActionResult Index(int? tableId)
        {
            // TODO: Logic load menu và thông tin bàn sẽ được implement sau
            ViewBag.TableId = tableId;
            return View();
        }

        /// <summary>
        /// Thêm món vào giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult AddToCart(int menuItemId, int soLuong, int tableId)
        {
            // TODO: Logic thêm vào giỏ hàng sẽ được implement sau
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
        }

        /// <summary>
        /// Gửi đơn hàng đến bếp
        /// </summary>
        [HttpPost]
        public IActionResult SubmitOrder(Order order)
        {
            // TODO: Logic gửi đơn đến bếp sẽ được implement sau
            if (ModelState.IsValid)
            {
                return Json(new { success = true, message = "Đơn hàng đã được gửi đến bếp" });
            }
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }
}

