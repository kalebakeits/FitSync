using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class CompositeHeartbeatIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_service_heartbeats_instance_id",
                table: "service_heartbeats");

            migrationBuilder.CreateIndex(
                name: "IX_service_heartbeats_instance_id_service_type",
                table: "service_heartbeats",
                columns: new[] { "instance_id", "service_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_service_heartbeats_instance_id_service_type",
                table: "service_heartbeats");

            migrationBuilder.CreateIndex(
                name: "IX_service_heartbeats_instance_id",
                table: "service_heartbeats",
                column: "instance_id",
                unique: true);
        }
    }
}
