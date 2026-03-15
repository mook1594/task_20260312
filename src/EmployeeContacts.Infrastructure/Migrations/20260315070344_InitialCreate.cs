using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeContacts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
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

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "Email", "Joined", "Name", "PhoneNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "2024-01-01T00:00:00.0000000+00:00", "charles@clovf.com", "2018-03-07", "김철수", "01075312468", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "2024-01-01T00:00:00.0000000+00:00", "matilda@clovf.com", "2021-04-28", "박영희", "01087654321", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "2024-01-01T00:00:00.0000000+00:00", "kildong.hong@clovf.com", "2015-08-15", "홍길동", "01012345678", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "2024-01-01T00:00:00.0000000+00:00", "clo@clovf.com", "2012-01-05", "김클로", "0101111-2424", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "2024-01-01T00:00:00.0000000+00:00", "md@clovf.com", "2013-07-01", "박마블", "0103535-7979", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "2024-01-01T00:00:00.0000000+00:00", "connect@clovf.com", "2019-12-05", "홍커넥", "0108531-7942", "2024-01-01T00:00:00.0000000+00:00" }
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
