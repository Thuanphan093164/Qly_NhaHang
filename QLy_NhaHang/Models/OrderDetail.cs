using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho chi tiết món trong đơn hàng
    /// </summary>
    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Đơn hàng không được để trống")]
        [Column("MaDonHang")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Món ăn không được để trống")]
        [Column("MaMon")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Column("SoLuong")]
        public int Quantity { get; set; }

        [Required]
        [Column("Gia", TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; } // Lưu giá bán tại thời điểm gọi món

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem? MenuItem { get; set; }
    }
}

