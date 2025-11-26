using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCredentialsToSeparateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "garmin_uploader_configs");

            migrationBuilder.DropColumn(name: "password", table: "zwift_fetcher_configs");

            migrationBuilder.DropColumn(name: "username", table: "zwift_fetcher_configs");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255
            );

            migrationBuilder.CreateTable(
                name: "user_credentials",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        user_id = table.Column<Guid>(type: "uuid", nullable: false),
                        service_type = table.Column<string>(
                            type: "character varying(50)",
                            maxLength: 50,
                            nullable: false
                        ),
                        username = table.Column<string>(
                            type: "character varying(500)",
                            maxLength: 500,
                            nullable: false
                        ),
                        password = table.Column<string>(
                            type: "character varying(1000)",
                            maxLength: 1000,
                            nullable: false
                        ),
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
                    table.PrimaryKey("PK_user_credentials", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_credentials_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_credentials_user_id_service_type",
                table: "user_credentials",
                columns: new[] { "user_id", "service_type" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "user_credentials");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "zwift_fetcher_configs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "zwift_fetcher_configs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500
            );

            migrationBuilder.CreateTable(
                name: "garmin_uploader_configs",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        user_id = table.Column<Guid>(type: "uuid", nullable: false),
                        created_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        password = table.Column<string>(type: "text", nullable: false),
                        updated_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        username = table.Column<string>(type: "text", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_garmin_uploader_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_garmin_uploader_configs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_garmin_uploader_configs_user_id",
                table: "garmin_uploader_configs",
                column: "user_id",
                unique: true
            );
        }
    }
}
