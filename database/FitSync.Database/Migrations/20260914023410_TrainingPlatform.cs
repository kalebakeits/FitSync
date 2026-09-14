using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class TrainingPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_service_heartbeats_instance_id",
                table: "service_heartbeats");

            migrationBuilder.DropColumn(
                name: "service_metadata",
                table: "scheduled_workouts");

            migrationBuilder.DropColumn(
                name: "service_type",
                table: "scheduled_workouts");

            migrationBuilder.AddColumn<DateTime>(
                name: "pending_publish_at",
                table: "scheduled_workouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "planned_duration_seconds",
                table: "scheduled_workouts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "publish_claimed_at",
                table: "scheduled_workouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "avg_heart_rate",
                table: "activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "avg_power",
                table: "activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "distance_meters",
                table: "activities",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_seconds",
                table: "activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "scheduled_workout_id",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sport",
                table: "activities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "auto_publish_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sport_category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_publish_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_auto_publish_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_workout_publications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_workout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_metadata = table.Column<string>(type: "jsonb", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_workout_publications", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_workout_publications_scheduled_workouts_scheduled~",
                        column: x => x.scheduled_workout_id,
                        principalTable: "scheduled_workouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_heartbeats_instance_id_service_type",
                table: "service_heartbeats",
                columns: new[] { "instance_id", "service_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activities_scheduled_workout_id",
                table: "activities",
                column: "scheduled_workout_id");

            migrationBuilder.CreateIndex(
                name: "IX_auto_publish_settings_user_id_service_type_sport_category",
                table: "auto_publish_settings",
                columns: new[] { "user_id", "service_type", "sport_category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_workout_publications_scheduled_workout_id_service~",
                table: "scheduled_workout_publications",
                columns: new[] { "scheduled_workout_id", "service_type" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_scheduled_workouts_scheduled_workout_id",
                table: "activities",
                column: "scheduled_workout_id",
                principalTable: "scheduled_workouts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_scheduled_workouts_scheduled_workout_id",
                table: "activities");

            migrationBuilder.DropTable(
                name: "auto_publish_settings");

            migrationBuilder.DropTable(
                name: "scheduled_workout_publications");

            migrationBuilder.DropIndex(
                name: "IX_service_heartbeats_instance_id_service_type",
                table: "service_heartbeats");

            migrationBuilder.DropIndex(
                name: "IX_activities_scheduled_workout_id",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "pending_publish_at",
                table: "scheduled_workouts");

            migrationBuilder.DropColumn(
                name: "planned_duration_seconds",
                table: "scheduled_workouts");

            migrationBuilder.DropColumn(
                name: "publish_claimed_at",
                table: "scheduled_workouts");

            migrationBuilder.DropColumn(
                name: "avg_heart_rate",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "avg_power",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "distance_meters",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "duration_seconds",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "scheduled_workout_id",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "sport",
                table: "activities");

            migrationBuilder.AddColumn<string>(
                name: "service_metadata",
                table: "scheduled_workouts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_type",
                table: "scheduled_workouts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_heartbeats_instance_id",
                table: "service_heartbeats",
                column: "instance_id",
                unique: true);
        }
    }
}
