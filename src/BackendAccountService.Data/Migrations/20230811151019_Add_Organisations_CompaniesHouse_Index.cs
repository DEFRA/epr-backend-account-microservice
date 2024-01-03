using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Organisations_CompaniesHouse_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Organisations_CompaniesHouseNumber",
                table: "Organisations",
                column: "CompaniesHouseNumber",
                filter: "[CompaniesHouseNumber] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_CompaniesHouseNumber",
                table: "Organisations");
        }
    }
}
