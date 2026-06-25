using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserDestinationConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDestinationConfigs",
                columns: table =>
                    new
                    {
                        UserId = table.Column<Guid>(type: "uuid", nullable: false),
                        SourceServiceType = table.Column<string>(type: "text", nullable: false),
                        DestinationServiceType = table.Column<string>(type: "text", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_UserDestinationConfigs",
                        x =>
                            new
                            {
                                x.UserId,
                                x.SourceServiceType,
                                x.DestinationServiceType
                            }
                    );
                    table.ForeignKey(
                        name: "FK_UserDestinationConfigs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserDestinationConfigs_UserId_SourceServiceType_Destination~",
                table: "UserDestinationConfigs",
                columns: new[] { "UserId", "SourceServiceType", "DestinationServiceType" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserDestinationConfigs");
        }
    }
}
