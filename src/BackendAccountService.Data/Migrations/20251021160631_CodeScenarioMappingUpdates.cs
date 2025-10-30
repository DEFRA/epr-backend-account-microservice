using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class CodeScenarioMappingUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CodeScenarioMapping",
                columns: new[] { "Id", "Active", "CodeStatusConfigId", "ScenarioReferenceId" },
                values: new object[,]
                {
                    { 19, true, 18, 18 },
                    { 20, true, 19, 19 },
                    { 21, true, 20, 20 },
                    { 22, true, 21, 21 }
                });

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 18,
                column: "ObligationFlag",
                value: "Not Obligated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CodeScenarioMapping",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CodeScenarioMapping",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CodeScenarioMapping",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "CodeScenarioMapping",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 18,
                column: "ObligationFlag",
                value: "3b - Not Obligated");
        }
    }
}
