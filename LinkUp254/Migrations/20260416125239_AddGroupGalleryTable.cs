using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupGalleryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupGallery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedById = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupGallery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupGallery_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupGallery_GroupId",
                table: "GroupGallery",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGallery_IsActive",
                table: "GroupGallery",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGallery_UploadedById",
                table: "GroupGallery",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupGallery");
        }
    }
}
