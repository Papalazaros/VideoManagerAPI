using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class Initial5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "PlaylistVideos");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_PlaylistId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "Rooms");

            migrationBuilder.CreateTable(
                name: "RoomVideos",
                columns: table => new
                {
                    RoomId = table.Column<int>(nullable: false),
                    VideoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomVideos", x => new { x.RoomId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_RoomVideos_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomVideos_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "VideoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomVideos_VideoId",
                table: "RoomVideos",
                column: "VideoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomVideos");

            migrationBuilder.AddColumn<int>(
                name: "PlaylistId",
                table: "Rooms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.PlaylistId);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistVideos",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "int", nullable: false),
                    VideoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistVideos", x => new { x.PlaylistId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_PlaylistVideos_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistVideos_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "VideoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_PlaylistId",
                table: "Rooms",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CreatedByUserId",
                table: "Playlists",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ModifiedByUserId",
                table: "Playlists",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistVideos_VideoId",
                table: "PlaylistVideos",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Playlists_PlaylistId",
                table: "Rooms",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
