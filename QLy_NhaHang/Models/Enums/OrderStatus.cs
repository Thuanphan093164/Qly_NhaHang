namespace QLy_NhaHang.Models.Enums
{
    /// <summary>
    /// Enum định nghĩa trạng thái đơn hàng
    /// </summary>
    public enum OrderStatus
    {
        New = 0,        // Đơn mới
        Processing = 1, // Đang xử lý
        Served = 2,     // Đã phục vụ
        Paid = 3        // Đã thanh toán
    }
}

