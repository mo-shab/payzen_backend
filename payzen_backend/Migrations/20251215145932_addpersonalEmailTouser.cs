using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class addpersonalEmailTouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailPersonal",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailPersonal",
                table: "Users",
                column: "EmailPersonal",
                filter: "[DeletedAt] IS NULL AND [EmailPersonal] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailPersonal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailPersonal",
                table: "Users");
        }
    }
}
