using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Local_Authority_EndpointCode_Data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrganisationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Waste Collection Authority" });

            migrationBuilder.InsertData(
                table: "OrganisationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "Waste Disposal Authority" });

            migrationBuilder.InsertData(
                table: "OrganisationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "Waste Collection Authority & Waste Disposal Authority" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
