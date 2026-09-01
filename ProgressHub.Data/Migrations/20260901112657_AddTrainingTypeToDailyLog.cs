using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingTypeToDailyLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainingType",
                table: "DailyLog",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainingType",
                table: "DailyLog");
        }
    }
}
