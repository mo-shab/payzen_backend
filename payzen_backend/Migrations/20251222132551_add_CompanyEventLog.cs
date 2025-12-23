using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace payzen_backend.Migrations
{
    /// <inheritdoc />
    public partial class add_CompanyEventLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyEventLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    eventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    oldValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    oldValueId = table.Column<int>(type: "int", nullable: true),
                    newValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    newValueId = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    createdBy = table.Column<int>(type: "int", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyEventLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyEventLog_Company_companyId",
                        column: x => x.companyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyEventLog_Users_createdBy",
                        column: x => x.createdBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEventLog_companyId",
                table: "CompanyEventLog",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEventLog_createdBy",
                table: "CompanyEventLog",
                column: "createdBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyEventLog");
        }
    }
}
