using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    public partial class NewsletterMakeUserIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "NewsletterSubscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterSubscriptions_Users_UserId",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_NewsletterSubscriptions_UserId",
                table: "NewsletterSubscriptions");

       
            migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT * FROM sys.indexes 
            WHERE name = 'IX_NewsletterSubscriptions_Email' 
            AND object_id = OBJECT_ID('NewsletterSubscriptions')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_NewsletterSubscriptions_Email] ON [NewsletterSubscriptions] ([Email])
        END");

            
            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterSubscriptions_Users_UserId",
                table: "NewsletterSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterSubscriptions_Users_UserId",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_NewsletterSubscriptions_Email",
                table: "NewsletterSubscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptions_UserId",
                table: "NewsletterSubscriptions",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterSubscriptions_Users_UserId",
                table: "NewsletterSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "NewsletterSubscriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}