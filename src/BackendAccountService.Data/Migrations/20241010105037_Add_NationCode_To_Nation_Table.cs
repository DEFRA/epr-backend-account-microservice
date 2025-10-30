using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_NationCode_To_Nation_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationCode",
                table: "Nations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Nations",
                keyColumn: "Id",
                keyValue: 0,
                column: "NationCode",
                value: "");

            migrationBuilder.UpdateData(
                table: "Nations",
                keyColumn: "Id",
                keyValue: 1,
                column: "NationCode",
                value: "GB-ENG");

            migrationBuilder.UpdateData(
                table: "Nations",
                keyColumn: "Id",
                keyValue: 2,
                column: "NationCode",
                value: "GB-NIR");

            migrationBuilder.UpdateData(
                table: "Nations",
                keyColumn: "Id",
                keyValue: 3,
                column: "NationCode",
                value: "GB-SCT");

            migrationBuilder.UpdateData(
                table: "Nations",
                keyColumn: "Id",
                keyValue: 4,
                column: "NationCode",
                value: "GB-WLS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationCode",
                table: "Nations");
        }
    }
}
