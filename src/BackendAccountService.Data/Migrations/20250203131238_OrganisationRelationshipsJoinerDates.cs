using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationRelationshipsJoinerDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JoinerDate",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingTypeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportingType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ReportingType",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[,]
                {
                    { 0, "", "Not Set" },
                    { 1, "SE", "Self" },
                    { 2, "GR", "Group" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_ReportingType_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId",
                principalTable: "ReportingType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_ReportingType_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "ReportingType");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "JoinerDate",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "ReportingTypeId",
                table: "OrganisationRelationships");
        }
    }
}
