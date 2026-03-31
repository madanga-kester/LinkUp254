using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpCodesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OtpCodes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OtpCodes");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "OtpCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "OtpCodes");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OtpCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "OtpCodes",
                type: "datetime2",
                nullable: true);
        }
    }
}
