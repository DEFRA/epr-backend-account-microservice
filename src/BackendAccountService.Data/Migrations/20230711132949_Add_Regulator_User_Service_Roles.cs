using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Regulator_User_Service_Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "Key", "Name" },
                values: new object[] { 2, "Extended Producer Responsibility - Regulating", "Regulating", "EPR Regulating" });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 4, "Regulator Admin Service Role", "Regulator.Admin", "Regulator Admin", 2 });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 5, "Regulator Basic Service Role", "Regulator.Basic", "Regulator Basic", 2 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
