using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Country_nationality_id",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAddress_Country_CountryId",
                table: "EmployeeAddress");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Country");

            migrationBuilder.RenameColumn(
                name: "nationality_id",
                table: "Employee",
                newName: "CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_nationality_id",
                table: "Employee",
                newName: "IX_Employee_CountryId");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "EmployeeAddress",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Country_CountryId",
                table: "Employee",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAddress_Country_CountryId",
                table: "EmployeeAddress",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Country_CountryId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAddress_Country_CountryId",
                table: "EmployeeAddress");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "Employee",
                newName: "nationality_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_CountryId",
                table: "Employee",
                newName: "IX_Employee_nationality_id");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "EmployeeAddress",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Country",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Country_nationality_id",
                table: "Employee",
                column: "nationality_id",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAddress_Country_CountryId",
                table: "EmployeeAddress",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
