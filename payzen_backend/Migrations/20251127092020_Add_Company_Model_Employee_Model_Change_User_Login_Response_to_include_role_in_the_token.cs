using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class Add_Company_Model_Employee_Model_Change_User_Login_Response_to_include_role_in_the_token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Users",
                newName: "EmployeeId");

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    company_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    city_id = table.Column<int>(type: "int", nullable: true),
                    country_id = table.Column<int>(type: "int", nullable: true),
                    ice_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    cnss_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    if_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    rc_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    rib_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    phone_number = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    managedby_company_id = table.Column<int>(type: "int", nullable: true),
                    is_cabinet_expert = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    modified_by = table.Column<int>(type: "int", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_Company_managedby_company_id",
                        column: x => x.managedby_company_id,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    cin_number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    phone = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    manager_id = table.Column<int>(type: "int", nullable: true),
                    status_id = table.Column<int>(type: "int", nullable: true),
                    gender_id = table.Column<int>(type: "int", nullable: true),
                    nationality_id = table.Column<int>(type: "int", nullable: true),
                    education_level_id = table.Column<int>(type: "int", nullable: true),
                    marital_status_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    modified_by = table.Column<int>(type: "int", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_Company_company_id",
                        column: x => x.company_id,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_Employee_manager_id",
                        column: x => x.manager_id,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_managedby_company_id",
                table: "Company",
                column: "managedby_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_company_id",
                table: "Employee",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_manager_id",
                table: "Employee",
                column: "manager_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employee_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employee_EmployeeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmployeeId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Users",
                newName: "CompanyId");
        }
    }
}
