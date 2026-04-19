using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupCoverImageSchemaUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GroupCoverImages",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupCoverImages_GroupId_IsActive",
                table: "GroupCoverImages",
                columns: new[] { "GroupId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupCoverImages_UploadedBy",
                table: "GroupCoverImages",
                column: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupCoverImages_GroupId_IsActive",
                table: "GroupCoverImages");

            migrationBuilder.DropIndex(
                name: "IX_GroupCoverImages_UploadedBy",
                table: "GroupCoverImages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GroupCoverImages");
        }
    }
}
