using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFitnessGoalToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FitnessGoal",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FitnessGoal",
                table: "Users");
        }
    }
}
