using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_heartbeats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    instance_id = table.Column<string>(type: "text", nullable: false),
                    hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_heartbeat_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    error_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_heartbeats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    verification_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reset_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    reset_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_activity_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fit_file_data = table.Column<byte[]>(type: "bytea", nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    claimed_by = table.Column<string>(type: "text", nullable: true),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processing_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processing_completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activity_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activity_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activity_metadata = table.Column<string>(type: "jsonb", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    last_error_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_activities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    auth_data = table.Column<string>(type: "text", nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lookup_value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_integrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processed_activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_activity_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fetched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_processed_activities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fetcher_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    next_fetch_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    worker_lock_id = table.Column<string>(type: "text", nullable: true),
                    lock_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fetch_interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fetcher_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_fetcher_configs_integrations_integration_id",
                        column: x => x.integration_id,
                        principalTable: "integrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_activities_activity_date",
                table: "activities",
                column: "activity_date");

            migrationBuilder.CreateIndex(
                name: "idx_activities_claimed_by",
                table: "activities",
                column: "claimed_by");

            migrationBuilder.CreateIndex(
                name: "idx_activities_status",
                table: "activities",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_id",
                table: "activities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_user_id_external_activity_id_source",
                table: "activities",
                columns: new[] { "user_id", "external_activity_id", "source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fetcher_configs_integration_id",
                table: "fetcher_configs",
                column: "integration_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fetcher_configs_next_fetch_time",
                table: "fetcher_configs",
                column: "next_fetch_time");

            migrationBuilder.CreateIndex(
                name: "IX_integrations_lookup_value",
                table: "integrations",
                column: "lookup_value");

            migrationBuilder.CreateIndex(
                name: "IX_integrations_user_id_service_type",
                table: "integrations",
                columns: new[] { "user_id", "service_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_processed_activities_user_source",
                table: "processed_activities",
                columns: new[] { "user_id", "source" });

            migrationBuilder.CreateIndex(
                name: "IX_processed_activities_user_id_external_activity_id_source",
                table: "processed_activities",
                columns: new[] { "user_id", "external_activity_id", "source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_service_heartbeats_last_heartbeat",
                table: "service_heartbeats",
                columns: new[] { "service_type", "last_heartbeat_at" });

            migrationBuilder.CreateIndex(
                name: "idx_service_heartbeats_service_type",
                table: "service_heartbeats",
                column: "service_type");

            migrationBuilder.CreateIndex(
                name: "IX_service_heartbeats_instance_id",
                table: "service_heartbeats",
                column: "instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_session_id",
                table: "sessions",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "fetcher_configs");

            migrationBuilder.DropTable(
                name: "processed_activities");

            migrationBuilder.DropTable(
                name: "service_heartbeats");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "integrations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
