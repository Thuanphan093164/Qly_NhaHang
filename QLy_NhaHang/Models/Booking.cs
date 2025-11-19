using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.Models
{
    /// <summary>
    /// Model đại diện cho đặt bàn từ xa (Remote User)
    /// </summary>
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [Column("Ma")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên khách hàng không được vượt quá 50 ký tự")]
        [Column("TenKhach")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Column("SoDienThoai")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đặt bàn không được để trống")]
        [DataType(DataType.DateTime)]
        [Column("GioDat")]
        public DateTime BookingTime { get; set; }

        [Required(ErrorMessage = "Số người không được để trống")]
        [Range(1, 50, ErrorMessage = "Số người phải từ 1 đến 50")]
        [Column("SoNguoi")]
        public int GuestCount { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Column("GhiChu")]
        public string? Note { get; set; }

        [Required]
        [Column("TrangThai")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [DataType(DataType.DateTime)]
        [Column("NgayTao")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
