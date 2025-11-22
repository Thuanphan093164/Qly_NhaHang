namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// Request model để đánh dấu món đã hoàn thành
    /// </summary>
    public class MarkOrderDetailCompleteRequest
    {
        public int OrderDetailId { get; set; }
        public bool IsCompleted { get; set; }
    }
}

