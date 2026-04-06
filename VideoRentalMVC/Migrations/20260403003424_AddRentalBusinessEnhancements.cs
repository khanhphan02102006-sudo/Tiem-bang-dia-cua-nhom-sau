using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRentalMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalBusinessEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HanTra",
                table: "Thues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PhiTreHan",
                table: "Thues",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TongTien",
                table: "Thues",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HanTra",
                table: "Thues");

            migrationBuilder.DropColumn(
                name: "PhiTreHan",
                table: "Thues");

            migrationBuilder.DropColumn(
                name: "TongTien",
                table: "Thues");
        }
    }
}
