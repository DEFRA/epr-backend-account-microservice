using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_New_Compliance_Schemes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: new[] { "Id", "CompaniesHouseNumber", "Name", "NationId" },
                values: new object[,]
                {
                    { 51, "05695937", "Beyondly (NIEA)", 2 },
                    { 52, "00946107", "BiffPack (NIEA)", 2 },
                    { 53, "04559478", "Comply with Clarity (NIEA)", 2 },
                    { 54, "SC331930", "Co2 Compliance (NIEA)", 2 },
                    { 55, "04164355", "ComplyPak (NIEA)", 2 },
                    { 56, "04713606", "Ecosurety (NIEA)", 2 },
                    { 57, "05859725", "ERP (NIEA)", 2 },
                    { 58, "04592964", "Kite Environmental Solutions (NIEA)", 2 },
                    { 59, "SC300070", "Leafpak (NIEA)", 2 },
                    { 60, "07699232", "Paperpak (NIEA)", 2 },
                    { 61, "03417947", "Veolia (NIEA)", 2 },
                    { 62, "04748329", "Recycling Lives Compliance Services (NIEA)", 2 },
                    { 63, "06355083", "REPIC (NIEA)", 2 },
                    { 64, "04015442", "Smart Comply (NIEA)", 2 },
                    { 65, "04835772", "Synergy Compliance (Northern Ireland Environment Agency)", 2 },
                    { 66, "06929701", "Pennine-Pack (Northern Ireland Environment Agency)", 2 },
                    { 67, "03985811", "Wastepack UK (NIEA)", 2 },
                    { 68, "07688691", "Valpak (NIEA)", 2 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 68);
        }
    }
}
