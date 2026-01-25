using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseumSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToExhibits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Exhibits",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Exhibits");
        }
    }
}
