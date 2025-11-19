using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho danh mục món ăn
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự")]
        [Column("Ten")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("DangHoatDong")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}

