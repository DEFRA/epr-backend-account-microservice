using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Change_ComplianceScheme_NationId_To_Nullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NationId",
                table: "ComplianceSchemes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 1,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 3,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 5,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 6,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 9,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 11,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 13,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 16,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 18,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 19,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 22,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 26,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 27,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 30,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 32,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 34,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 37,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 38,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 39,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 41,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 43,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 45,
                column: "NationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 46,
                column: "NationId",
                value: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NationId",
                table: "ComplianceSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 1,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 3,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 5,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 6,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 9,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 11,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 13,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 16,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 18,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 19,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 22,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 26,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 27,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 30,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 32,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 34,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 37,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 38,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 39,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 41,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 43,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 45,
                column: "NationId",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemes",
                keyColumn: "Id",
                keyValue: 46,
                column: "NationId",
                value: 0);
        }
    }
}
