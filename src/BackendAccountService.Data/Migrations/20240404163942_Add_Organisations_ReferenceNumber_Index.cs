using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Organisations_ReferenceNumber_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Organisations_ReferenceNumber",
                table: "Organisations",
                column: "ReferenceNumber",
                filter: "[IsDeleted] = CAST(0 AS bit)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_ReferenceNumber",
                table: "Organisations");
        }
    }
}
