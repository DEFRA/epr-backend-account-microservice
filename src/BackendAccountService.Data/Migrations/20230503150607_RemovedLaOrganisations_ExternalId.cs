using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class RemovedLaOrganisations_ExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LaOrganisations_ExternalId",
                table: "LaOrganisations");
            
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "LaOrganisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "LaOrganisations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");
            
            migrationBuilder.CreateIndex(
                name: "IX_LaOrganisations_ExternalId",
                table: "LaOrganisations",
                column: "ExternalId",
                unique: true);            
        }
    }
}
