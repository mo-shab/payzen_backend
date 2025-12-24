using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class add_Action_to_Permission_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "Permissions");
        }
    }
}
