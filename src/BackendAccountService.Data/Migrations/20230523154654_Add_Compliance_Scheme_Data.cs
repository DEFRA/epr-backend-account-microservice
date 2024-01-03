using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Compliance_Scheme_Data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceSchemes_Organisations_OrganisationId",
                table: "ComplianceSchemes");

            migrationBuilder.DropIndex(
                name: "IX_ComplianceSchemes_OrganisationId",
                table: "ComplianceSchemes");

            migrationBuilder.DropColumn(
                name: "OrganisationId",
                table: "ComplianceSchemes");

            migrationBuilder.AddColumn<string>(
                name: "CompaniesHouseNumber",
                table: "ComplianceSchemes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: new[] { "Id", "CompaniesHouseNumber", "Name" },
                values: new object[,]
                {
                    { 1, "9100134", "Sustain Drinks Packaging Partnership" },
                    { 2, "946107", "Biffpack (Northern Ireland Environment Agency)" },
                    { 3, "946107", "Biffpack (Environment Agency)" },
                    { 4, "4559478", "Comply with Clarity" },
                    { 5, "4559478", "Comply with Clarity (Northern Ireland Environment Agency)" },
                    { 6, "SC331930", "Co2 Compliance (Northern Ireland Environment Agency)" },
                    { 7, "SC331930", "Co2 Compliance (Scottish Environment Protection Agency)" },
                    { 8, "5695937", "Comply Direct (Environment Agency)" },
                    { 9, "5695937", "Comply Direct (Northern Ireland Environment Agency)" },
                    { 10, "4164355", "ComplyPak (Environment Agency)" },
                    { 11, "4164355", "ComplyPak (Northern Ireland Environment Agency)" },
                    { 12, "4713606", "Ecosurety (Environment Agency)" },
                    { 13, "4713606", "Ecosurety (Northern Ireland Environment Agency)" },
                    { 14, "SC499366", "Ecosurety Scotland" },
                    { 15, "5859725", "ERP UK Ltd" },
                    { 16, "5859725", "ERP UK Ltd Northern Ireland Environment Agency" },
                    { 17, "4592964", "Kite Environmental Solutions" },
                    { 18, "4592964", "Kite Environmental Solutions (Northern Ireland Environment Agency)" },
                    { 19, "NI044560", "Nipak Ltd (Northern Ireland Environment Agency)" },
                    { 20, "SC300070", "Scotpak (Scottish Environment Protection Agency)" },
                    { 21, "7699232", "Paperpak (Environment Agency)" },
                    { 22, "7699232", "Paperpak (Northern Ireland Environment Agency)" },
                    { 23, "3417947", "Veolia (Environment Agency)" },
                    { 24, "SC394249", "Veolia - Scottish Environment Protection Agency" },
                    { 25, "4543366", "Recycle Wales" },
                    { 26, "SC225789", "Recycle-Pak (Northern Ireland Environment Agency)" },
                    { 27, "SC225789", "Recycle-Pak (Scottish Environment Protection Agency)" },
                    { 28, "4748329", "Recycling Lives Compliance Services" },
                    { 29, "6355083", "REPIC" },
                    { 30, "6355083", "REPIC NI" },
                    { 31, "4015442", "Smart Comply (Environment Agency)" },
                    { 32, "4015442", "Smart Comply (Northern Ireland Environment Agency)" },
                    { 33, "4835772", "Synergy Compliance (Environment Agency)" },
                    { 34, "4835772", "Synergy Compliance (Northern Ireland Environment Agency)" },
                    { 35, "6929701", "Ethical Compliance" },
                    { 36, "6929701", "Pennine-Pack (Environment Agency)" },
                    { 37, "6929701", "Pennine-Pack (Northern Ireland Environment Agency)" },
                    { 38, "3985811", "Wastepack (Scottish Environment Protection Agency)" },
                    { 39, "3985811", "Wastepack UK (Northern Ireland Environment Agency)" },
                    { 40, "7688691", "Valpak (Environment Agency)" },
                    { 41, "7688691", "Valpak (Northern Ireland Environment Agency)" },
                    { 42, "SC245145", "Valpak Scotland Limited (Scottish Environment Protection Agency)" }
                });

            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: new[] { "Id", "CompaniesHouseNumber", "Name" },
                values: new object[,]
                {
                    { 43, "2215767", "Veolia Environmental Services (Northern Ireland Environment Agency)" },
                    { 44, "6043169", "Packcare" },
                    { 45, "SC174113", "Compliance Link (Scottish Environment Protection Agency)" },
                    { 46, "SC174113", "SWS Compak" },
                    { 47, "SC174113", "Wastepack (Scottish Environment Protection Agency)" },
                    { 48, "4168907", "Enpack" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DropColumn(
                name: "CompaniesHouseNumber",
                table: "ComplianceSchemes");

            migrationBuilder.AddColumn<int>(
                name: "OrganisationId",
                table: "ComplianceSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemes_OrganisationId",
                table: "ComplianceSchemes",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceSchemes_Organisations_OrganisationId",
                table: "ComplianceSchemes",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id");
        }
    }
}
