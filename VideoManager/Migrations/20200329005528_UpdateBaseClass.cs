using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class UpdateBaseClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ModifiedBy",
                table: "Videos",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Videos",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Videos",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModifiedBy",
                table: "Rooms",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Rooms",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModifiedBy",
                table: "Playlists",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Playlists",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Playlists",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Playlists",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CreatedByUserUserId",
                table: "Videos",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ModifiedByUserUserId",
                table: "Videos",
                column: "ModifiedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CreatedByUserUserId",
                table: "Rooms",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ModifiedByUserUserId",
                table: "Rooms",
                column: "ModifiedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CreatedByUserUserId",
                table: "Playlists",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ModifiedByUserUserId",
                table: "Playlists",
                column: "ModifiedByUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_CreatedByUserUserId",
                table: "Playlists",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_ModifiedByUserUserId",
                table: "Playlists",
                column: "ModifiedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_CreatedByUserUserId",
                table: "Rooms",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_ModifiedByUserUserId",
                table: "Rooms",
                column: "ModifiedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Users_CreatedByUserUserId",
                table: "Videos",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Users_ModifiedByUserUserId",
                table: "Videos",
                column: "ModifiedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_CreatedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_ModifiedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_CreatedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_ModifiedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Users_CreatedByUserUserId",
                table: "Videos");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Users_ModifiedByUserUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_CreatedByUserUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_ModifiedByUserUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_CreatedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_ModifiedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_CreatedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_ModifiedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Playlists");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
