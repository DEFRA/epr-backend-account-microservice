using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reprocessor_Exported_Standard_User_Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 12, null, "Re-Ex.StandardUser", "Standard User", 4 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 12);
        }
    }
}
