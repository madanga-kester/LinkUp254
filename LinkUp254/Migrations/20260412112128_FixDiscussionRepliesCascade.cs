using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class FixDiscussionRepliesCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoinRequests_Users_UserId",
                table: "GroupJoinRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_Users_SenderId",
                table: "GroupMessages");

            migrationBuilder.CreateTable(
                name: "GroupDiscussionReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDiscussionReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupDiscussionReactions_Discussion",
                        column: x => x.TargetId,
                        principalTable: "GroupDiscussions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupDiscussionReactions_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupDiscussionReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscussionId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ParentReplyId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDiscussionReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupDiscussionReplies_GroupDiscussions_DiscussionId",
                        column: x => x.DiscussionId,
                        principalTable: "GroupDiscussions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupDiscussionReplies_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupDiscussionReactions_TargetId",
                table: "GroupDiscussionReactions",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDiscussionReactions_UserId",
                table: "GroupDiscussionReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDiscussionReplies_AuthorId",
                table: "GroupDiscussionReplies",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDiscussionReplies_DiscussionId",
                table: "GroupDiscussionReplies",
                column: "DiscussionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoinRequests_Users_UserId",
                table: "GroupJoinRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_Users_SenderId",
                table: "GroupMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoinRequests_Users_UserId",
                table: "GroupJoinRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_Users_SenderId",
                table: "GroupMessages");

            migrationBuilder.DropTable(
                name: "GroupDiscussionReactions");

            migrationBuilder.DropTable(
                name: "GroupDiscussionReplies");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoinRequests_Users_UserId",
                table: "GroupJoinRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_Users_SenderId",
                table: "GroupMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
