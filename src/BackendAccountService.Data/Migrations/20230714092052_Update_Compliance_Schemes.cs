using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Update_Compliance_Schemes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Biffpack");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Co2 Compliance");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "ComplyPak");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Ecosurety (EA)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 16,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "IsDeleted", "Name" },
                values: new object[] { true, "Kite Environmental Solutions" });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 19,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 20,
                column: "Name",
                value: "Scotpak");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 21,
                column: "Name",
                value: "Paperpak");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 22,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 23,
                column: "Name",
                value: "Veolia - EA");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 24,
                column: "Name",
                value: "Veolia (SEPA)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 26,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 27,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 30,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 31,
                column: "Name",
                value: "Smart Comply (EA)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 32,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 33,
                column: "Name",
                value: "Synergy Compliance");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 34,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 36,
                column: "Name",
                value: "Pennine-Pack");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 37,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 38,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 39,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 40,
                column: "Name",
                value: "Valpak");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 41,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 42,
                column: "Name",
                value: "Valpak Scotland Ltd");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 43,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 45,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 46,
                column: "IsDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 47,
                column: "Name",
                value: "Wastepack");

            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: new[] { "Id", "CompaniesHouseNumber", "Name" },
                values: new object[,]
                {
                    { 49, "SC300070", "Leafpak" },
                    { 50, "03985811", "Wastepack (EA)" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Biffpack (Northern Ireland Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Co2 Compliance (Scottish Environment Protection Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "ComplyPak (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Ecosurety (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 16,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "IsDeleted", "Name" },
                values: new object[] { false, "Kite Environmental Solutions (Northern Ireland Environment Agency)" });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 19,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 20,
                column: "Name",
                value: "Scotpak (Scottish Environment Protection Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 21,
                column: "Name",
                value: "Paperpak (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 22,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 23,
                column: "Name",
                value: "Veolia (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 24,
                column: "Name",
                value: "Veolia - Scottish Environment Protection Agency");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 26,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 27,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 30,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 31,
                column: "Name",
                value: "Smart Comply (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 32,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 33,
                column: "Name",
                value: "Synergy Compliance (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 34,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 36,
                column: "Name",
                value: "Pennine-Pack (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 37,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 38,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 39,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 40,
                column: "Name",
                value: "Valpak (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 41,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 42,
                column: "Name",
                value: "Valpak Scotland Limited (Scottish Environment Protection Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 43,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 45,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 46,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 47,
                column: "Name",
                value: "Wastepack (Scottish Environment Protection Agency)");
        }
    }
}
