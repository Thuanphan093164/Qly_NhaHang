namespace QLy_NhaHang.Models.Enums
{
    /// <summary>
    /// Enum định nghĩa trạng thái đặt bàn
    /// </summary>
    public enum BookingStatus
    {
        Pending = 0,    // Chờ xác nhận
        Confirmed = 1,  // Đã xác nhận
        CheckedIn = 2,  // Đã check-in
        Cancelled = 3   // Đã hủy
    }
}

