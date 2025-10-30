using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reprocessor_Exporter_Service_Role_Descriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: "Manage team, submit registration and accreditation");

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Read only");

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "Manage team, submit registration and accreditation");

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: "Submit registration and apply for accreditation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: null);
        }
    }
}
