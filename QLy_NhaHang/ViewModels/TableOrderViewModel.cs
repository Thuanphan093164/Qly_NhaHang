namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// ViewModel cho bàn có đơn hàng trong quản lý bếp
    /// </summary>
    public class TableOrderViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public DateTime LatestOrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
    }
}

