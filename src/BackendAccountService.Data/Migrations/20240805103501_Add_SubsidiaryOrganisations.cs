using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_SubsidiaryOrganisations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrganisationRelationshipTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrganisationRelationshipTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<int>(
                name: "OrganisationRegistrationTypeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "SubsidiaryOrganisations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<int>(type: "int", nullable: false),
                    SubsidiaryId = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsidiaryOrganisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubsidiaryOrganisations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_SecondOrganisationId",
                table: "OrganisationRelationships",
                column: "SecondOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_SubsidiaryOrganisations_OrganisationId",
                table: "SubsidiaryOrganisations",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_Organisations_SecondOrganisationId",
                table: "OrganisationRelationships",
                column: "SecondOrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_Organisations_SecondOrganisationId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "SubsidiaryOrganisations");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_SecondOrganisationId",
                table: "OrganisationRelationships");

            migrationBuilder.AlterColumn<int>(
                name: "OrganisationRegistrationTypeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "OrganisationRelationshipTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Holding" });

            migrationBuilder.InsertData(
                table: "OrganisationRelationshipTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Subsidary" });
        }
    }
}
