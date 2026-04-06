using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VideoRentalMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhimAndThueData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bangs_Phims_PhimMaPhim",
                table: "Bangs");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Bangs_MaBangNavigationMaBang",
                table: "Thues");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Khaches_MaKhachNavigationMaKhach",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Thues_MaBangNavigationMaBang",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Thues_MaKhachNavigationMaKhach",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Bangs_PhimMaPhim",
                table: "Bangs");

            migrationBuilder.DropColumn(
                name: "MaBangNavigationMaBang",
                table: "Thues");

            migrationBuilder.DropColumn(
                name: "MaKhachNavigationMaKhach",
                table: "Thues");

            migrationBuilder.DropColumn(
                name: "PhimMaPhim",
                table: "Bangs");

            migrationBuilder.InsertData(
                table: "Khaches",
                columns: new[] { "MaKhach", "DiaChi", "DienThoai", "IdentityUserId", "TenKhach" },
                values: new object[,]
                {
                    { 1, "HCM", "0123456789", null, "Nguyen Van A" },
                    { 2, "HN", "0987654321", null, "Tran Thi B" }
                });

            migrationBuilder.InsertData(
                table: "Phims",
                columns: new[] { "MaPhim", "GiaVon", "MoTa", "NamSanXuat", "NuocSanXuat", "PhimBoLe", "TenPhim", "TheLoai" },
                values: new object[,]
                {
                    { 1, 100000m, "Phim hành động", 2020, "VN", false, "Phim A", "Action" },
                    { 2, 200000m, "Phim tình cảm", 2021, "US", true, "Phim B", "Romance" }
                });

            migrationBuilder.InsertData(
                table: "Bangs",
                columns: new[] { "MaBang", "MaPhim", "TenBang", "TinhTrang" },
                values: new object[,]
                {
                    { 1, 1, "Bang A", "Mới" },
                    { 2, 2, "Bang B", "Cũ" }
                });

            migrationBuilder.InsertData(
                table: "Thues",
                columns: new[] { "MaThue", "DaTraTienThue", "MaBang", "MaKhach", "NgayThue", "NgayTra" },
                values: new object[,]
                {
                    { 1, false, 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, true, 2, 2, new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Thues_MaBang",
                table: "Thues",
                column: "MaBang");

            migrationBuilder.CreateIndex(
                name: "IX_Thues_MaKhach",
                table: "Thues",
                column: "MaKhach");

            migrationBuilder.CreateIndex(
                name: "IX_Bangs_MaPhim",
                table: "Bangs",
                column: "MaPhim");

            migrationBuilder.AddForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs",
                column: "MaPhim",
                principalTable: "Phims",
                principalColumn: "MaPhim",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues",
                column: "MaBang",
                principalTable: "Bangs",
                principalColumn: "MaBang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Khaches_MaKhach",
                table: "Thues",
                column: "MaKhach",
                principalTable: "Khaches",
                principalColumn: "MaKhach",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Khaches_MaKhach",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Thues_MaBang",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Thues_MaKhach",
                table: "Thues");

            migrationBuilder.DropIndex(
                name: "IX_Bangs_MaPhim",
                table: "Bangs");

            migrationBuilder.DeleteData(
                table: "Thues",
                keyColumn: "MaThue",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Thues",
                keyColumn: "MaThue",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Bangs",
                keyColumn: "MaBang",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bangs",
                keyColumn: "MaBang",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Khaches",
                keyColumn: "MaKhach",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Khaches",
                keyColumn: "MaKhach",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Phims",
                keyColumn: "MaPhim",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Phims",
                keyColumn: "MaPhim",
                keyValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "MaBangNavigationMaBang",
                table: "Thues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaKhachNavigationMaKhach",
                table: "Thues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhimMaPhim",
                table: "Bangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Thues_MaBangNavigationMaBang",
                table: "Thues",
                column: "MaBangNavigationMaBang");

            migrationBuilder.CreateIndex(
                name: "IX_Thues_MaKhachNavigationMaKhach",
                table: "Thues",
                column: "MaKhachNavigationMaKhach");

            migrationBuilder.CreateIndex(
                name: "IX_Bangs_PhimMaPhim",
                table: "Bangs",
                column: "PhimMaPhim");

            migrationBuilder.AddForeignKey(
                name: "FK_Bangs_Phims_PhimMaPhim",
                table: "Bangs",
                column: "PhimMaPhim",
                principalTable: "Phims",
                principalColumn: "MaPhim",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Bangs_MaBangNavigationMaBang",
                table: "Thues",
                column: "MaBangNavigationMaBang",
                principalTable: "Bangs",
                principalColumn: "MaBang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Khaches_MaKhachNavigationMaKhach",
                table: "Thues",
                column: "MaKhachNavigationMaKhach",
                principalTable: "Khaches",
                principalColumn: "MaKhach",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
