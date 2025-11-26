using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitSync.Database.Migrations
{
    /// <inheritdoc />
    public partial class Email_Hash_For_Comparison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_hash",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "email_hash", table: "users");
        }
    }
}
