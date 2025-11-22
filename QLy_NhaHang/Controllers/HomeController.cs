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
        /// Trang chủ
        /// </summary>
        public IActionResult Index()
        {
            // Trang chủ không hiển thị thực đơn, chỉ hiển thị Hero Banner và Info Section
            var emptyViewModel = new HomeIndexViewModel();
            return View(emptyViewModel);
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
        /// Trang xem thực đơn theo danh mục
        /// </summary>
        public async Task<IActionResult> Menu(string? category)
        {
            try
            {
                // Map category slug sang tên category trong database
                var categoryName = MapCategorySlugToName(category);
                
                // Lấy danh sách menu items theo category
                var menuItems = await GetMenuItemsByCategoryAsync(categoryName);
                
                // Map sang ViewModel
                var viewModel = new MenuViewModel
                {
                    CategoryName = categoryName ?? "Tất cả",
                    CategorySlug = category,
                    MenuItems = menuItems.Select(item => new MenuItemViewModel
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

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu menu");
                var emptyViewModel = new MenuViewModel
                {
                    CategoryName = "Tất cả",
                    MenuItems = new List<MenuItemViewModel>()
                };
                return View(emptyViewModel);
            }
        }

        /// <summary>
        /// Trang Món ăn
        /// </summary>
        public async Task<IActionResult> MonAn()
        {
            return await GetCategoryMenuAsync("Món ăn", "MonAn");
        }

        /// <summary>
        /// Trang Thức uống
        /// </summary>
        public async Task<IActionResult> ThucUong()
        {
            return await GetCategoryMenuAsync("Thức uống", "ThucUong");
        }

        /// <summary>
        /// Trang Rượu
        /// </summary>
        public async Task<IActionResult> Ruou()
        {
            return await GetCategoryMenuAsync("Rượu", "Ruou");
        }

        /// <summary>
        /// Helper method để lấy menu items theo category và trả về view
        /// </summary>
        private async Task<IActionResult> GetCategoryMenuAsync(string categoryName, string viewName)
        {
            try
            {
                // Lấy danh sách menu items theo category
                var menuItems = await GetMenuItemsByCategoryAsync(categoryName);
                
                // Map sang ViewModel
                var viewModel = new MenuViewModel
                {
                    CategoryName = categoryName,
                    CategorySlug = null,
                    MenuItems = menuItems.Select(item => new MenuItemViewModel
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

                return View(viewName, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải dữ liệu menu cho {categoryName}");
                var emptyViewModel = new MenuViewModel
                {
                    CategoryName = categoryName,
                    MenuItems = new List<MenuItemViewModel>()
                };
                return View(viewName, emptyViewModel);
            }
        }

        /// <summary>
        /// Map category slug từ URL sang tên category trong database
        /// </summary>
        private string? MapCategorySlugToName(string? categorySlug)
        {
            if (string.IsNullOrEmpty(categorySlug))
                return null;

            return categorySlug.ToLower() switch
            {
                "mon-an" => "Món ăn",
                "thuc-uong" => "Thức uống",
                "ruou" => "Rượu",
                _ => null
            };
        }

        /// <summary>
        /// Lấy danh sách menu items theo category
        /// </summary>
        private async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string? categoryName)
        {
            var query = _context.MenuItems
                .Where(m => m.IsActive == true)
                .Include(m => m.Category)
                .AsQueryable();

            // Nếu có category, lọc theo category
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(m => m.Category != null && m.Category.Name == categoryName);
            }

            return await query
                .OrderBy(m => m.Category != null ? m.Category.Name : "")
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
