using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRentalMVC.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues");

            migrationBuilder.AddForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs",
                column: "MaPhim",
                principalTable: "Phims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues",
                column: "MaBang",
                principalTable: "Bangs",
                principalColumn: "Id",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs",
                column: "MaPhim",
                principalTable: "Phims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues",
                column: "MaBang",
                principalTable: "Bangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
