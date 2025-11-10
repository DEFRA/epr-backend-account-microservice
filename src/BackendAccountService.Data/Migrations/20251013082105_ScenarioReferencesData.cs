using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScenarioReferencesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "ObligationFlag" },
                values: new object[] { "Producer who meets thresholds independently has left group. Holding Company remains responsible for obligation due to Mid Year change", "3b - Not Obligated" });

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 19,
                column: "Description",
                value: "Producer Registered late.");

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 20,
                column: "Description",
                value: "Other - Joiner");

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "ObligationFlag" },
                values: new object[] { "Other - Leaver", "Not Obligated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "ObligationFlag" },
                values: new object[] { "Became a producer mid-year - all scenarios", "Obligated" });

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 19,
                column: "Description",
                value: "Became a producer mid-year - all scenarios");

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 20,
                column: "Description",
                value: "Became a producer mid-year - all scenarios");

            migrationBuilder.UpdateData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "ObligationFlag" },
                values: new object[] { "Became a producer mid-year - all scenarios", "Obligated" });
        }
    }
}
