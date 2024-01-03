using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Update_Compliance_Scheme_For_Beyondly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Beyondly");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsDeleted",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Comply Direct (Environment Agency)");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsDeleted",
                value: false);
        }
    }
}
