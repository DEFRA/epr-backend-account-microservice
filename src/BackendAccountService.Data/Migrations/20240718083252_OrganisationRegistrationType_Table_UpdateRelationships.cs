using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class OrganisationRegistrationType_Table_UpdateRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RelationFromDate",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "OrganisationRegistrationTypeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrganisationRegistrationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRegistrationTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "OrganisationRegistrationTypes",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[] { 0, null, "Not Set" });

            migrationBuilder.InsertData(
                table: "OrganisationRegistrationTypes",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[] { 1, null, "Group" });

            migrationBuilder.InsertData(
                table: "OrganisationRegistrationTypes",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[] { 2, null, "Individual" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_OrganisationRegistrationTypeId",
                table: "OrganisationRelationships",
                column: "OrganisationRegistrationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_OrganisationRegistrationTypes_OrganisationRegistrationTypeId",
                table: "OrganisationRelationships",
                column: "OrganisationRegistrationTypeId",
                principalTable: "OrganisationRegistrationTypes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_OrganisationRegistrationTypes_OrganisationRegistrationTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "OrganisationRegistrationTypes");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_OrganisationRegistrationTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "OrganisationRegistrationTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RelationFromDate",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");
        }
    }
}
