using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_NationId_To_ComplianceScheme_And_Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationId",
                table: "ComplianceSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 2,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 4,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Co2 Compliance (SEPA)", 3 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 8,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 10,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 12,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Ecosurety Scotland (SEPA)", 3 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 15,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 17,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Scotpak (SEPA)", 3 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 21,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Veolia (EA)", 1 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 24,
                column: "NationId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Recycle Wales (NRW)", 4 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 28,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 29,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 31,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 33,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 35,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 36,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 40,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Valpak Scotland Ltd (SEPA)", 3 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 44,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Name", "NationId" },
                values: new object[] { "Wastepack GB (SEPA)", 3 });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 48,
                column: "NationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 49,
                column: "NationId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 50,
                column: "NationId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemes_NationId",
                table: "ComplianceSchemes",
                column: "NationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceSchemes_Nations_NationId",
                table: "ComplianceSchemes",
                column: "NationId",
                principalTable: "Nations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceSchemes_Nations_NationId",
                table: "ComplianceSchemes");

            migrationBuilder.DropIndex(
                name: "IX_ComplianceSchemes_NationId",
                table: "ComplianceSchemes");

            migrationBuilder.DropColumn(
                name: "NationId",
                table: "ComplianceSchemes");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Co2 Compliance");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Ecosurety Scotland");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 20,
                column: "Name",
                value: "Scotpak");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 23,
                column: "Name",
                value: "Veolia - EA");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 25,
                column: "Name",
                value: "Recycle Wales");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 42,
                column: "Name",
                value: "Valpak Scotland Ltd");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 47,
                column: "Name",
                value: "Wastepack");
        }
    }
}
