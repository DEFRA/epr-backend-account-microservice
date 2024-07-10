using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_OrganisationRelationshipType_OrganisationRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganisationRelationshipTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRelationshipTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstOrganisationId = table.Column<int>(type: "int", nullable: false),
                    SecondOrganisationId = table.Column<int>(type: "int", nullable: false),
                    OrganisationRelationshipTypeId = table.Column<int>(type: "int", nullable: false),
                    RelationFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelationToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelationExpiryReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedByOrganisationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationRelationships_OrganisationRelationshipTypes_OrganisationRelationshipTypeId",
                        column: x => x.OrganisationRelationshipTypeId,
                        principalTable: "OrganisationRelationshipTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationRelationships_Organisations_FirstOrganisationId",
                        column: x => x.FirstOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationRelationships_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "OrganisationRelationshipTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Parent" },
                    { 2, "Holding" },
                    { 3, "Subsidary" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_FirstOrganisationId",
                table: "OrganisationRelationships",
                column: "FirstOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_LastUpdatedById",
                table: "OrganisationRelationships",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_OrganisationRelationshipTypeId",
                table: "OrganisationRelationships",
                column: "OrganisationRelationshipTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "OrganisationRelationshipTypes");
        }
    }
}
