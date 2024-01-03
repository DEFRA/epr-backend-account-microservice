using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class AddLAOrganisationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LaOrganisations_ExternalId",
                table: "LaOrganisations");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "LaOrganisations");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdatedOn",
                table: "LaOrganisations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "LaOrganisations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "LaOrganisations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");

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

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdatedOn",
                table: "LaOrganisations",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "LaOrganisations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "LaOrganisations",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

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
