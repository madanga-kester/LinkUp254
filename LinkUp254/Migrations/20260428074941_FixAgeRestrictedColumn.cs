using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUp254.Migrations
{
    /// <inheritdoc />
    public partial class FixAgeRestrictedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Both AgeRestricted and MinAge already exist in database - no action needed
            // This migration records the schema state without modifying it
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed - columns are part of baseline schema
        }
    }
}