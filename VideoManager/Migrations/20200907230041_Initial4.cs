using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class Initial4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms");

            migrationBuilder.AlterColumn<int>(
                name: "PlaylistId",
                table: "Rooms",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms");

            migrationBuilder.AlterColumn<int>(
                name: "PlaylistId",
                table: "Rooms",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
