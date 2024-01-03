using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_LaPayment_User_Service_Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "Key", "Name" },
                values: new object[] { 3, "Local Authority Payment Service", "LaPayment", "Local Authority Payment Service" });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 6, null, "LaPayment.UserAdministrator", "User Administrator", 3 });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 7, null, "LaPayment.BasicUser", "Basic User", 3 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
