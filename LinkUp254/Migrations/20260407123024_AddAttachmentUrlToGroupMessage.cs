using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentUrlToGroupMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "GroupMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "GroupMessages");
        }
    }
}
