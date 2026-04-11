using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillPath.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperiencePointsToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExperiencePoints",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperiencePoints",
                table: "Tasks");
        }
    }
}
