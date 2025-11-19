using Microsoft.EntityFrameworkCore;
using QLy_NhaHang.Models;

namespace QLy_NhaHang.Data
{
    /// <summary>
    /// DbContext chính của ứng dụng quản lý nhà hàng
    /// </summary>
    public class QlyNhaHangContext : DbContext
    {
        public QlyNhaHangContext(DbContextOptions<QlyNhaHangContext> options)
            : base(options)
        {
        }

        // DbSet cho các entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình cho Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Cấu hình cho MenuItem
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasOne(m => m.Category)
                      .WithMany(c => c.MenuItems)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Category nếu còn MenuItem

                entity.HasIndex(e => e.Name);
            });

            // Cấu hình cho Table
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Cấu hình cho Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasIndex(e => e.BookingTime);
                entity.HasIndex(e => e.PhoneNumber);
            });

            // Cấu hình cho Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Table)
                      .WithMany(t => t.Orders)
                      .HasForeignKey(o => o.TableId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Table nếu còn Order

                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);
            });

            // Cấu hình cho OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Order thì xóa luôn OrderDetail

                entity.HasOne(od => od.MenuItem)
                      .WithMany(m => m.OrderDetails)
                      .HasForeignKey(od => od.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa MenuItem nếu còn OrderDetail

                entity.HasIndex(e => new { e.OrderId, e.MenuItemId });
            });
        }
    }
}

