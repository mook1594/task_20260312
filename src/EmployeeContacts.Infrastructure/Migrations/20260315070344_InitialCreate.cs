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
                    { new Guid("019cf071-5dd6-7d05-b05b-a9ef638ad993"), "2024-01-01T00:00:00.0000000+00:00", "charles@clovf.com", "2018-03-07", "김철수", "01075312468", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("019cf073-2030-78e3-9d64-18d848a55be6"), "2024-01-01T00:00:00.0000000+00:00", "matilda@clovf.com", "2021-04-28", "박영희", "01087654321", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("019cf074-070f-7db0-ac38-b646b9259540"), "2024-01-01T00:00:00.0000000+00:00", "kildong.hong@clovf.com", "2015-08-15", "홍길동", "01012345678", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("019cf074-070f-744d-ae44-e417096e7011"), "2024-01-01T00:00:00.0000000+00:00", "clo@clovf.com", "2012-01-05", "김클로", "0101111-2424", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("019cf076-35be-7a4e-82ea-c7a6267fc856"), "2024-01-01T00:00:00.0000000+00:00", "md@clovf.com", "2013-07-01", "박마블", "0103535-7979", "2024-01-01T00:00:00.0000000+00:00" },
                    { new Guid("019cf076-35bf-72ba-b655-f2da3e1ada22"), "2024-01-01T00:00:00.0000000+00:00", "connect@clovf.com", "2019-12-05", "홍커넥", "0108531-7942", "2024-01-01T00:00:00.0000000+00:00" }
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
