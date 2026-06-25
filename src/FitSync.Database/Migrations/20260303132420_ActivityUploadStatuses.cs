using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class ActivityUploadStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "idx_activities_claimed_by", table: "activities");

            migrationBuilder.DropIndex(name: "idx_activities_status", table: "activities");

            migrationBuilder.DropColumn(name: "claimed_at", table: "activities");

            migrationBuilder.DropColumn(name: "claimed_by", table: "activities");

            migrationBuilder.DropColumn(name: "last_error", table: "activities");

            migrationBuilder.DropColumn(name: "last_error_at", table: "activities");

            migrationBuilder.DropColumn(name: "processing_completed_at", table: "activities");

            migrationBuilder.DropColumn(name: "retry_count", table: "activities");

            migrationBuilder.DropColumn(name: "status", table: "activities");

            migrationBuilder.RenameColumn(
                name: "processing_started_at",
                table: "activities",
                newName: "deleted_at"
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "activities",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.CreateTable(
                name: "activity_upload_statuses",
                columns: table =>
                    new
                    {
                        activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                        destination_service_type = table.Column<string>(
                            type: "character varying(50)",
                            maxLength: 50,
                            nullable: false
                        ),
                        status = table.Column<int>(type: "integer", nullable: false),
                        claimed_by = table.Column<string>(type: "text", nullable: true),
                        claimed_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        processing_started_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        processing_completed_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        retry_count = table.Column<int>(type: "integer", nullable: false),
                        last_error = table.Column<string>(type: "text", nullable: true),
                        last_error_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_activity_upload_statuses",
                        x => new { x.activity_id, x.destination_service_type }
                    );
                    table.ForeignKey(
                        name: "FK_activity_upload_statuses_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_activity_upload_statuses_activity_id",
                table: "activity_upload_statuses",
                column: "activity_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_activity_upload_statuses_claimed_by",
                table: "activity_upload_statuses",
                column: "claimed_by"
            );

            migrationBuilder.CreateIndex(
                name: "idx_activity_upload_statuses_status",
                table: "activity_upload_statuses",
                column: "status"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "activity_upload_statuses");

            migrationBuilder.DropColumn(name: "is_deleted", table: "activities");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "activities",
                newName: "processing_started_at"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "claimed_at",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "claimed_by",
                table: "activities",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "activities",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "last_error_at",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "processing_completed_at",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                table: "activities",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "activities",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "idx_activities_claimed_by",
                table: "activities",
                column: "claimed_by"
            );

            migrationBuilder.CreateIndex(
                name: "idx_activities_status",
                table: "activities",
                column: "status"
            );
        }
    }
}
