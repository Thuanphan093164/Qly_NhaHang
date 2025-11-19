using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho bàn ăn trong nhà hàng
    /// </summary>
    [Table("Tables")]
    public class Table
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên bàn không được để trống")]
        [StringLength(20, ErrorMessage = "Tên bàn không được vượt quá 20 ký tự")]
        [Column("Ten")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sức chứa không được để trống")]
        [Range(1, 20, ErrorMessage = "Sức chứa phải từ 1 đến 20 người")]
        [Column("SucChua")]
        public int Capacity { get; set; }

        [Required]
        [Column("TrangThai")]
        public TableStatus CurrentStatus { get; set; } = TableStatus.Free;

        [Required]
        [Column("An")]
        public bool IsHidden { get; set; } = false;

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
