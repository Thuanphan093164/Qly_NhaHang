using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Models;
using QLy_NhaHang.Models.Enums;

namespace QLy_NhaHang.Data
{
    /// <summary>
    /// Class để khởi tạo dữ liệu mẫu cho database
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Seed dữ liệu mẫu cho Tables
        /// </summary>
        public static async Task SeedTablesAsync(QlyNhaHangContext context)
        {
            // Kiểm tra xem đã có dữ liệu chưa
            if (await context.Tables.AnyAsync())
            {
                return; // Đã có dữ liệu, không seed nữa
            }

            var tables = new List<Table>
            {
                // Bàn trống (màu xanh)
                new Table { Name = "Bàn 1", Capacity = 2, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 2", Capacity = 4, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 3", Capacity = 4, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 4", Capacity = 6, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 5", Capacity = 2, CurrentStatus = TableStatus.Free, IsHidden = false },
                
                // Bàn có khách (màu đỏ)
                new Table { Name = "Bàn 6", Capacity = 4, CurrentStatus = TableStatus.Occupied, IsHidden = false },
                new Table { Name = "Bàn 7", Capacity = 8, CurrentStatus = TableStatus.Occupied, IsHidden = false },
                
                // Bàn đã đặt trước (màu vàng)
                new Table { Name = "Bàn 8", Capacity = 4, CurrentStatus = TableStatus.Reserved, IsHidden = false },
                new Table { Name = "Bàn 9", Capacity = 6, CurrentStatus = TableStatus.Reserved, IsHidden = false },
                
                // Thêm một số bàn trống nữa
                new Table { Name = "Bàn 10", Capacity = 2, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 11", Capacity = 4, CurrentStatus = TableStatus.Free, IsHidden = false },
                new Table { Name = "Bàn 12", Capacity = 8, CurrentStatus = TableStatus.Free, IsHidden = false },
            };

            await context.Tables.AddRangeAsync(tables);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed tất cả dữ liệu mẫu
        /// </summary>
        public static async Task SeedAllAsync(QlyNhaHangContext context)
        {
            await SeedTablesAsync(context);
            // Có thể thêm seed cho Categories, MenuItems sau
        }
    }
}

