using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class AlignEventRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_UsersId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_UsersId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UsersId1",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsersId1",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_UsersId1",
                table: "Events",
                column: "UsersId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_UsersId1",
                table: "Events",
                column: "UsersId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
