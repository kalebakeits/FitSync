using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "oauth_clients",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        client_id = table.Column<string>(
                            type: "character varying(255)",
                            maxLength: 255,
                            nullable: false
                        ),
                        client_secret_hash = table.Column<string>(
                            type: "character varying(500)",
                            maxLength: 500,
                            nullable: false
                        ),
                        name = table.Column<string>(
                            type: "character varying(255)",
                            maxLength: 255,
                            nullable: false
                        ),
                        redirect_uris = table.Column<string[]>(type: "text[]", nullable: false),
                        created_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_clients", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "oauth_codes",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        code = table.Column<string>(
                            type: "character varying(500)",
                            maxLength: 500,
                            nullable: false
                        ),
                        client_id = table.Column<Guid>(type: "uuid", nullable: false),
                        user_id = table.Column<Guid>(type: "uuid", nullable: false),
                        redirect_uri = table.Column<string>(
                            type: "character varying(500)",
                            maxLength: 500,
                            nullable: false
                        ),
                        expires_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        used_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        created_at = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_oauth_codes_oauth_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "oauth_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_oauth_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_oauth_codes_client_id",
                table: "oauth_codes",
                column: "client_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_oauth_codes_user_id",
                table: "oauth_codes",
                column: "user_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "oauth_codes");

            migrationBuilder.DropTable(name: "oauth_clients");
        }
    }
}
