namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// Request model cho việc cập nhật trạng thái bàn nhanh
    /// </summary>
    public class UpdateTableStatusRequest
    {
        public int TableId { get; set; }
        public int NewStatus { get; set; }
    }
}

