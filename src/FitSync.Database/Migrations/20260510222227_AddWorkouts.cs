using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workouts",
                columns: table =>
                    new
                    {
                        id = table.Column<Guid>(type: "uuid", nullable: false),
                        user_id = table.Column<Guid>(type: "uuid", nullable: false),
                        name = table.Column<string>(
                            type: "character varying(255)",
                            maxLength: 255,
                            nullable: false
                        ),
                        tags = table.Column<List<string>>(type: "text[]", nullable: false),
                        file_path = table.Column<string>(
                            type: "character varying(500)",
                            maxLength: 500,
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
                    table.PrimaryKey("PK_workouts", x => x.id);
                    table.ForeignKey(
                        name: "FK_workouts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_workouts_user_id",
                table: "workouts",
                column: "user_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "workouts");
        }
    }
}
