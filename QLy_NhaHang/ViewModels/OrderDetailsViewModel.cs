namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// ViewModel cho chi tiết đơn hàng của bàn
    /// </summary>
    public class OrderDetailsViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public List<OrderInfo> Orders { get; set; } = new List<OrderInfo>();
    }

    /// <summary>
    /// Thông tin đơn hàng
    /// </summary>
    public class OrderInfo
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public List<OrderDetailInfo> OrderDetails { get; set; } = new List<OrderDetailInfo>();
    }

    /// <summary>
    /// Thông tin chi tiết món trong đơn hàng
    /// </summary>
    public class OrderDetailInfo
    {
        public int OrderDetailId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public bool IsCompleted { get; set; }
    }
}

