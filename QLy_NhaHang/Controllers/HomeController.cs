using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Data;
using QLy_NhaHang.Models;
using QLy_NhaHang.ViewModels;

namespace QLy_NhaHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlyNhaHangContext _context;
        private const int BEST_SELLER_COUNT = 8;

        public HomeController(ILogger<HomeController> logger, QlyNhaHangContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Trang chủ - Hiển thị món ăn nổi bật
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var bestSellerItems = await GetBestSellerItemsAsync(BEST_SELLER_COUNT);
                var viewModel = MapToViewModel(bestSellerItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu trang chủ");
                var emptyViewModel = new HomeIndexViewModel();
                return View(emptyViewModel);
            }
        }

        /// <summary>
        /// Lấy danh sách món ăn nổi bật từ database
        /// </summary>
        private async Task<List<MenuItem>> GetBestSellerItemsAsync(int count)
        {
            return await _context.MenuItems
                .Where(m => m.IsActive == true)
                .Include(m => m.Category)
                .OrderByDescending(m => m.Id) // TODO: Có thể thay đổi logic sắp xếp theo số lượng đặt hàng
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Map từ Model sang ViewModel
        /// </summary>
        private HomeIndexViewModel MapToViewModel(List<MenuItem> items)
        {
            return new HomeIndexViewModel
            {
                BestSellerItems = items.Select(item => new MenuItemViewModel
                {
                    Id = item.Id,
                    Ten = item.Name,
                    MoTa = item.Description,
                    Gia = item.Price,
                    Anh = item.ImageUrl,
                    DonVi = item.Unit,
                    TenDanhMuc = item.Category?.Name
                }).ToList()
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Trang xem thực đơn tham khảo (cho khách từ xa)
        /// </summary>
        public IActionResult Menu()
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
