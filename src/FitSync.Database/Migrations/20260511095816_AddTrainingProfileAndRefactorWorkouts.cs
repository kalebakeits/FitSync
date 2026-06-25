using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProfileAndRefactorWorkouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "file_path", table: "workouts");

            migrationBuilder.AddColumn<string>(
                name: "schema",
                table: "workouts",
                type: "jsonb",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "sport",
                table: "workouts",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateTable(
                name: "training_profiles",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        user_id = table.Column<Guid>(type: "uuid", nullable: false),
                        ftp_watts = table.Column<int>(type: "integer", nullable: true),
                        cycling_threshold_hr = table.Column<int>(type: "integer", nullable: true),
                        cycling_max_hr = table.Column<int>(type: "integer", nullable: true),
                        running_threshold_hr = table.Column<int>(type: "integer", nullable: true),
                        running_max_hr = table.Column<int>(type: "integer", nullable: true),
                        pool_length_metres = table.Column<float>(type: "real", nullable: true),
                        swim_threshold_hr = table.Column<int>(type: "integer", nullable: true),
                        created_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        updated_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_profiles_user_id",
                table: "training_profiles",
                column: "user_id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "training_profiles");

            migrationBuilder.DropColumn(name: "schema", table: "workouts");

            migrationBuilder.DropColumn(name: "sport", table: "workouts");

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "workouts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
