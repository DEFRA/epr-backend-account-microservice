using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReportingTypeTableRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportingTypes");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ReportingTypes",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[,]
                {
                    { 0, "", "Not Set" },
                    { 1, "SE", "Self" },
                    { 2, "GR", "Group" }
                });
        }
    }
}
