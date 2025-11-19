using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLy_NhaHang.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsToVietnamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Categories_CategoryId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_MenuItems_MenuItemId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tables",
                newName: "Ten");

            migrationBuilder.RenameColumn(
                name: "IsHidden",
                table: "Tables",
                newName: "An");

            migrationBuilder.RenameColumn(
                name: "CurrentStatus",
                table: "Tables",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Tables",
                newName: "SucChua");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tables",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_Tables_Name",
                table: "Tables",
                newName: "IX_Tables_Ten");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Orders",
                newName: "TongTien");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "Orders",
                newName: "MaBan");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Orders",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "Orders",
                newName: "NgayDat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Orders",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                newName: "IX_Orders_MaBan");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                newName: "IX_Orders_TrangThai");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                newName: "IX_Orders_NgayDat");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderDetails",
                newName: "SoLuong");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "OrderDetails",
                newName: "Gia");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "OrderDetails",
                newName: "MaDonHang");

            migrationBuilder.RenameColumn(
                name: "MenuItemId",
                table: "OrderDetails",
                newName: "MaMon");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrderDetails",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_OrderId_MenuItemId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_MaDonHang_MaMon");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_MenuItemId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_MaMon");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "MenuItems",
                newName: "DonVi");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "MenuItems",
                newName: "Gia");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MenuItems",
                newName: "Ten");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "MenuItems",
                newName: "DangHoatDong");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "MenuItems",
                newName: "Anh");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "MenuItems",
                newName: "Mo_Ta");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "MenuItems",
                newName: "MaDanhMuc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MenuItems",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_Name",
                table: "MenuItems",
                newName: "IX_MenuItems_Ten");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                newName: "IX_MenuItems_MaDanhMuc");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "Ten");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categories",
                newName: "DangHoatDong");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                newName: "IX_Categories_Ten");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Bookings",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Bookings",
                newName: "SoDienThoai");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Bookings",
                newName: "GhiChu");

            migrationBuilder.RenameColumn(
                name: "GuestCount",
                table: "Bookings",
                newName: "SoNguoi");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "Bookings",
                newName: "TenKhach");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Bookings",
                newName: "NgayTao");

            migrationBuilder.RenameColumn(
                name: "BookingTime",
                table: "Bookings",
                newName: "GioDat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Bookings",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_PhoneNumber",
                table: "Bookings",
                newName: "IX_Bookings_SoDienThoai");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_BookingTime",
                table: "Bookings",
                newName: "IX_Bookings_GioDat");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Categories_MaDanhMuc",
                table: "MenuItems",
                column: "MaDanhMuc",
                principalTable: "Categories",
                principalColumn: "Ma",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_MenuItems_MaMon",
                table: "OrderDetails",
                column: "MaMon",
                principalTable: "MenuItems",
                principalColumn: "Ma",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_MaDonHang",
                table: "OrderDetails",
                column: "MaDonHang",
                principalTable: "Orders",
                principalColumn: "Ma",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_MaBan",
                table: "Orders",
                column: "MaBan",
                principalTable: "Tables",
                principalColumn: "Ma",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Categories_MaDanhMuc",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_MenuItems_MaMon",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_MaDonHang",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_MaBan",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Tables",
                newName: "CurrentStatus");

            migrationBuilder.RenameColumn(
                name: "Ten",
                table: "Tables",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SucChua",
                table: "Tables",
                newName: "Capacity");

            migrationBuilder.RenameColumn(
                name: "An",
                table: "Tables",
                newName: "IsHidden");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "Tables",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tables_Ten",
                table: "Tables",
                newName: "IX_Tables_Name");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "TongTien",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "NgayDat",
                table: "Orders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "MaBan",
                table: "Orders",
                newName: "TableId");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_TrangThai",
                table: "Orders",
                newName: "IX_Orders_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_NgayDat",
                table: "Orders",
                newName: "IX_Orders_OrderDate");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_MaBan",
                table: "Orders",
                newName: "IX_Orders_TableId");

            migrationBuilder.RenameColumn(
                name: "SoLuong",
                table: "OrderDetails",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "MaMon",
                table: "OrderDetails",
                newName: "MenuItemId");

            migrationBuilder.RenameColumn(
                name: "MaDonHang",
                table: "OrderDetails",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Gia",
                table: "OrderDetails",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "OrderDetails",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_MaMon",
                table: "OrderDetails",
                newName: "IX_OrderDetails_MenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_MaDonHang_MaMon",
                table: "OrderDetails",
                newName: "IX_OrderDetails_OrderId_MenuItemId");

            migrationBuilder.RenameColumn(
                name: "Ten",
                table: "MenuItems",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Mo_Ta",
                table: "MenuItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "MaDanhMuc",
                table: "MenuItems",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Gia",
                table: "MenuItems",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "DonVi",
                table: "MenuItems",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "DangHoatDong",
                table: "MenuItems",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Anh",
                table: "MenuItems",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "MenuItems",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_Ten",
                table: "MenuItems",
                newName: "IX_MenuItems_Name");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_MaDanhMuc",
                table: "MenuItems",
                newName: "IX_MenuItems_CategoryId");

            migrationBuilder.RenameColumn(
                name: "Ten",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DangHoatDong",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Ten",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Bookings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "TenKhach",
                table: "Bookings",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "SoNguoi",
                table: "Bookings",
                newName: "GuestCount");

            migrationBuilder.RenameColumn(
                name: "SoDienThoai",
                table: "Bookings",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "NgayTao",
                table: "Bookings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "GioDat",
                table: "Bookings",
                newName: "BookingTime");

            migrationBuilder.RenameColumn(
                name: "GhiChu",
                table: "Bookings",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "Bookings",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_SoDienThoai",
                table: "Bookings",
                newName: "IX_Bookings_PhoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_GioDat",
                table: "Bookings",
                newName: "IX_Bookings_BookingTime");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Categories_CategoryId",
                table: "MenuItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_MenuItems_MenuItemId",
                table: "OrderDetails",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
