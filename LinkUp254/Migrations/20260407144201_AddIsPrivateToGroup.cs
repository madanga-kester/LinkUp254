using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrivateToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_MemberId",
                table: "Groups",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_MemberId",
                table: "Groups",
                column: "MemberId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_MemberId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_MemberId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Groups");
        }
    }
}
