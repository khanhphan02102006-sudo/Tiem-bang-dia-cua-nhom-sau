using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRentalMVC.Migrations
{
    public partial class SyncModelAfterRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bangs_Phims_MaPhim",
                table: "Bangs");

            migrationBuilder.DropForeignKey(
                name: "FK_Khaches_AspNetUsers_IdentityUserId",
                table: "Khaches");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Bangs_MaBang",
                table: "Thues");

            migrationBuilder.DropForeignKey(
                name: "FK_Thues_Khaches_MaKhach",
                table: "Thues");

            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Khaches",
                table: "Khaches");

            migrationBuilder.DropIndex(
                name: "IX_Khaches_IdentityUserId",
                table: "Khaches");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Khaches");

            migrationBuilder.RenameTable(
                name: "Khaches",
                newName: "Khachs");

            migrationBuilder.RenameColumn(
                name: "MaThue",
                table: "Thues",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MaPhim",
                table: "Phims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MaBang",
                table: "Bangs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MaKhach",
                table: "Khachs",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Khachs",
                table: "Khachs",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Thues_Khachs_MaKhach",
                table: "Thues",
                column: "MaKhach",
                principalTable: "Khachs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
