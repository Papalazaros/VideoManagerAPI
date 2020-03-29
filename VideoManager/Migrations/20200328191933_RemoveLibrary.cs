using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class RemoveLibrary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Libraries_LibraryId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Libraries_LibraryId",
                table: "Videos");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Videos_LibraryId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Users_LibraryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "Videos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LibraryId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    LibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.LibraryId);
                    table.ForeignKey(
                        name: "FK_Libraries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_LibraryId",
                table: "Videos",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LibraryId",
                table: "Users",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId",
                table: "Libraries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Libraries_LibraryId",
                table: "Users",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "LibraryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Libraries_LibraryId",
                table: "Videos",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "LibraryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
