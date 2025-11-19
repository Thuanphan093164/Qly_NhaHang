namespace QLy_NhaHang.Models.Enums
{
    /// <summary>
    /// Enum định nghĩa trạng thái của bàn
    /// </summary>
    public enum TableStatus
    {
        Free = 0,       // Bàn trống
        Occupied = 1,  // Bàn đang có khách
        Reserved = 2   // Bàn đã được đặt trước
    }
}

