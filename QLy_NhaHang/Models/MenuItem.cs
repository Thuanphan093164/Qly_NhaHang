using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho món ăn trong menu
    /// </summary>
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        [StringLength(100, ErrorMessage = "Tên món không được vượt quá 100 ký tự")]
        [Column("Ten")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Column("Mo_Ta")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Column("Gia", TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Column("Anh")]
        public string? ImageUrl { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị không được vượt quá 20 ký tự")]
        [Column("DonVi")]
        public string Unit { get; set; } = "phần";

        [Required(ErrorMessage = "Danh mục không được để trống")]
        [Column("MaDanhMuc")]
        public int CategoryId { get; set; }

        [Required]
        [Column("DangHoatDong")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Navigation property cho OrderDetail
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
