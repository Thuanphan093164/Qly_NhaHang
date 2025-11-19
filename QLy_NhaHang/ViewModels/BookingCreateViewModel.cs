using System.ComponentModel.DataAnnotations;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.ViewModels
{
    /// <summary>
    /// ViewModel cho form đặt bàn
    /// </summary>
    public class BookingCreateViewModel
    {
        public int? MaBan { get; set; } // Mã bàn được chọn

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên khách hàng không được vượt quá 50 ký tự")]
        [Display(Name = "Tên khách hàng")]
        public string TenKhach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đặt bàn không được để trống")]
        [Display(Name = "Ngày và giờ đặt bàn")]
        public DateTime GioDat { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Số người không được để trống")]
        [Range(1, 50, ErrorMessage = "Số người phải từ 1 đến 50")]
        [Display(Name = "Số người")]
        public int SoNguoi { get; set; } = 2;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú (tùy chọn)")]
        public string? GhiChu { get; set; }
    }
}

