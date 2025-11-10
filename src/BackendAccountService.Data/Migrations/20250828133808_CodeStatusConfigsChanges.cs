using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class CodeStatusConfigsChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "Person becomes a producer as a result of carrying on the activities of an incapacitated producer.");

            migrationBuilder.InsertData(
                table: "CodeStatusConfigs",
                columns: new[] { "Id", "ClassificationId", "Code", "Description", "Enabled", "LegacyCode", "MappedOldCodes", "MatchType", "RequiresJoinerDate", "RequiresLeaverDate", "RequiresRegType" },
                values: new object[,]
                {
                    { 18, 1, "18", "Producer who meets thresholds independently has left group. Holding company remains responsible for obligation due to mid year change.", true, "", "", "ManualReview", true, false, false },
                    { 19, 1, "19", "Producer registered late.", true, "", "", "ManualReview", true, false, true },
                    { 20, 1, "20", "Other - Joiner.", true, "", "", "ManualReview", false, false, true },
                    { 21, 2, "21", "Other - Leaver.", true, "", "", "ManualReview", false, false, true }
                });

            migrationBuilder.InsertData(
                table: "ScenarioReferences",
                columns: new[] { "Id", "Active", "Description", "ObligationFlag", "ScenarioCode" },
                values: new object[,]
                {
                    { 19, true, "Became a producer mid-year - all scenarios", "Obligated", "all" },
                    { 20, true, "Became a producer mid-year - all scenarios", "Obligated", "all" },
                    { 21, true, "Became a producer mid-year - all scenarios", "Obligated", "all" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ScenarioReferences",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.UpdateData(
                table: "CodeStatusConfigs",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "Producer data changed due to MYC (e.g. insolvent acquisition).");
        }
    }
}
