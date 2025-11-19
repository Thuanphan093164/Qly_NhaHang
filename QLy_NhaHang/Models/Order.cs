using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho đơn gọi món tại bàn (In-house User)
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Bàn không được để trống")]
        [Column("MaBan")]
        public int TableId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("NgayDat")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column("TongTien", TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        [Column("TrangThai")]
        public OrderStatus Status { get; set; } = OrderStatus.New;

        // Navigation property
        [ForeignKey("TableId")]
        public virtual Table? Table { get; set; }

        // Navigation property cho OrderDetail
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
