using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Search_Organisations_By_SearchTerm_Query_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Name_ReferenceNumber_NationId",
                table: "Organisations",
                columns: new[] { "Name", "ReferenceNumber", "NationId" },
                filter: "[OrganisationTypeId] <> 6 AND [IsDeleted] = CAST(0 AS bit)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Name_ReferenceNumber_NationId",
                table: "Organisations");
        }
    }
}
