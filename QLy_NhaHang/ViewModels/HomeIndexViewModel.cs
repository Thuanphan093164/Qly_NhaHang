namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chủ
    /// </summary>
    public class HomeIndexViewModel
    {
        public List<MenuItemViewModel> BestSellerItems { get; set; } = new List<MenuItemViewModel>();
    }

    /// <summary>
    /// ViewModel cho món ăn hiển thị trên trang chủ
    /// </summary>
    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public string? Anh { get; set; }
        public string DonVi { get; set; } = "phần";
        public string? TenDanhMuc { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang Menu
    /// </summary>
    public class MenuViewModel
    {
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; } = new List<MenuItemViewModel>();
    }
}

