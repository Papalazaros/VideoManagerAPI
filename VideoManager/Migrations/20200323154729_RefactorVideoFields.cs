using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoManager.Migrations
{
    public partial class RefactorVideoFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Length",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "UserProvidedName",
                table: "Videos");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EncodeTime",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EncodedLength",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncodedType",
                table: "Videos",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Videos",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "OriginalLength",
                table: "Videos",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "OriginalType",
                table: "Videos",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncodeTime",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "EncodedLength",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "EncodedType",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "OriginalLength",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "OriginalType",
                table: "Videos");

            migrationBuilder.AddColumn<long>(
                name: "Length",
                table: "Videos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserProvidedName",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
