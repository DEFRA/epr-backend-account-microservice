using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reportyntypetableupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_ReportingType_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportingType",
                table: "ReportingType");

            migrationBuilder.RenameTable(
                name: "ReportingType",
                newName: "ReportingTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportingTypes",
                table: "ReportingTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_ReportingTypes_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId",
                principalTable: "ReportingTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_ReportingTypes_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportingTypes",
                table: "ReportingTypes");

            migrationBuilder.RenameTable(
                name: "ReportingTypes",
                newName: "ReportingType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportingType",
                table: "ReportingType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_ReportingType_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId",
                principalTable: "ReportingType",
                principalColumn: "Id");
        }
    }
}
