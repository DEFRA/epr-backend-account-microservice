using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class OrganisationRelationshipsDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdatedOn",
                table: "OrganisationRelationships",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "OrganisationRelationships",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "Key",
                value: "");

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Key",
                value: "GR");

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Key",
                value: "IN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "Key",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Key",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrganisationRegistrationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Key",
                value: null);
        }
    }
}
