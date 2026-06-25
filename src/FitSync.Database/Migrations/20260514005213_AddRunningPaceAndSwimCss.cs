using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRunningPaceAndSwimCss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "running_threshold_pace_seconds",
                table: "training_profiles",
                type: "integer",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "swim_css_seconds",
                table: "training_profiles",
                type: "integer",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "running_threshold_pace_seconds",
                table: "training_profiles"
            );

            migrationBuilder.DropColumn(name: "swim_css_seconds", table: "training_profiles");
        }
    }
}
