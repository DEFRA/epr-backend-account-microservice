using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Update_ComplianceSchemeMemberRemovalReasons_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IsActive", "Name" },
                values: new object[] { true, "Its turnover in the last financial year before the relevant date is less than or equal to £1 million" });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsActive",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IsActive", "Name" },
                values: new object[] { false, "Its turnover in the last financial year before the relevant date is less than or equal to �1 million" });

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsActive",
                value: false);

            migrationBuilder.UpdateData(
                table: "ComplianceSchemeMemberRemovalReasons",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsActive",
                value: false);
        }
    }
}
