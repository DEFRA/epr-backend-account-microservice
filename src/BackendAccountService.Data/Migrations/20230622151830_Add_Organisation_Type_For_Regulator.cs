using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Organisation_Type_For_Regulator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrganisationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "Regulators" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
