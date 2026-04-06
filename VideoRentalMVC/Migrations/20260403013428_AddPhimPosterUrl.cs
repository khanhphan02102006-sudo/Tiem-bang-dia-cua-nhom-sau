using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRentalMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddPhimPosterUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhBiaUrl",
                table: "Phims",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhBiaUrl",
                table: "Phims");
        }
    }
}
