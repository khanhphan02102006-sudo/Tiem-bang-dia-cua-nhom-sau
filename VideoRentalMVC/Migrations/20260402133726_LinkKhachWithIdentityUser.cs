using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRentalMVC.Migrations
{
    /// <inheritdoc />
    public partial class LinkKhachWithIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Khachs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Khachs_IdentityUserId",
                table: "Khachs",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Khachs_AspNetUsers_IdentityUserId",
                table: "Khachs",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Khachs_AspNetUsers_IdentityUserId",
                table: "Khachs");

            migrationBuilder.DropIndex(
                name: "IX_Khachs_IdentityUserId",
                table: "Khachs");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Khachs");
        }
    }
}
