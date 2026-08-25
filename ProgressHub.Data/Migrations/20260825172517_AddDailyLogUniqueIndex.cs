using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyLogUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyLog_UserId",
                table: "DailyLog");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLog_UserId_Date",
                table: "DailyLog",
                columns: new[] { "UserId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyLog_UserId_Date",
                table: "DailyLog");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLog_UserId",
                table: "DailyLog",
                column: "UserId");
        }
    }
}
