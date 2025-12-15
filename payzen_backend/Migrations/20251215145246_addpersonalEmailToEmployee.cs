using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class addpersonalEmailToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "personal_email",
                table: "Employee",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_personal_email",
                table: "Employee",
                column: "personal_email",
                filter: "[deleted_at] IS NULL AND [personal_email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employee_personal_email",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "personal_email",
                table: "Employee");
        }
    }
}
