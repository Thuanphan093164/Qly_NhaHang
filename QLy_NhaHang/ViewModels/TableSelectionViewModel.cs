using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// ViewModel cho việc chọn bàn
    /// </summary>
    public class TableSelectionViewModel
    {
        public List<TableViewModel> Tables { get; set; } = new List<TableViewModel>();
        public DateTime? SelectedDateTime { get; set; }
        public string CurrentFilter { get; set; } = "all";
    }

    /// <summary>
    /// ViewModel cho một bàn
    /// </summary>
    public class TableViewModel
    {
        public int Ma { get; set; }
        public string Ten { get; set; } = string.Empty;
        public int SucChua { get; set; }
        public TableStatus TrangThai { get; set; }
        public bool IsAvailable { get; set; } // Bàn có thể đặt được không
    }
}

