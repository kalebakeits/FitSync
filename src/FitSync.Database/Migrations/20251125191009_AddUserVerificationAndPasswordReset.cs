using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserVerificationAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "reset_token",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "reset_token_expires_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "verification_token",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "verification_token_expires_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_verified", table: "users");

            migrationBuilder.DropColumn(name: "reset_token", table: "users");

            migrationBuilder.DropColumn(name: "reset_token_expires_at", table: "users");

            migrationBuilder.DropColumn(name: "verification_token", table: "users");

            migrationBuilder.DropColumn(name: "verification_token_expires_at", table: "users");
        }
    }
}
