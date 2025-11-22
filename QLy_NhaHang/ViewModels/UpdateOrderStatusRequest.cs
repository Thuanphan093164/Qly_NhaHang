namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// Request model để cập nhật trạng thái đơn hàng
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public int Status { get; set; }
    }
}

