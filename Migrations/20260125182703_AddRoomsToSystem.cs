using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseumSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsToSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Exhibits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_RoomId",
                table: "Exhibits",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exhibits_Rooms_RoomId",
                table: "Exhibits",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exhibits_Rooms_RoomId",
                table: "Exhibits");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Exhibits_RoomId",
                table: "Exhibits");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Exhibits");
        }
    }
}
