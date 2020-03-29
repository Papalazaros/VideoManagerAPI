using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class UpdateBaseClassNaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "CreatedBy",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Playlists");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserId",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Playlists",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserId",
                table: "Playlists",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CreatedByUserId",
                table: "Videos",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ModifiedByUserId",
                table: "Videos",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CreatedByUserId",
                table: "Rooms",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ModifiedByUserId",
                table: "Rooms",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CreatedByUserId",
                table: "Playlists",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ModifiedByUserId",
                table: "Playlists",
                column: "ModifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_CreatedByUserId",
                table: "Playlists",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_ModifiedByUserId",
                table: "Playlists",
                column: "ModifiedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_CreatedByUserId",
                table: "Rooms",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_ModifiedByUserId",
                table: "Rooms",
                column: "ModifiedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Users_CreatedByUserId",
                table: "Videos",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Users_ModifiedByUserId",
                table: "Videos",
                column: "ModifiedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_CreatedByUserId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_ModifiedByUserId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_CreatedByUserId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_ModifiedByUserId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Users_CreatedByUserId",
                table: "Videos");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Users_ModifiedByUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_CreatedByUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_ModifiedByUserId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_CreatedByUserId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_ModifiedByUserId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_CreatedByUserId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_ModifiedByUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "Playlists");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Playlists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Playlists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Playlists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedByUserUserId",
                table: "Playlists",
                type: "uniqueidentifier",
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
    }
}
