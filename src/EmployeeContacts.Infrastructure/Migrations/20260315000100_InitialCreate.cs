using EmployeeContacts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeContacts.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260315000100_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Email = table.Column<string>(type: "TEXT", nullable: false),
                PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                Joined = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Email",
            table: "Employees",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Name",
            table: "Employees",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_PhoneNumber",
            table: "Employees",
            column: "PhoneNumber",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Employees");
    }
}
