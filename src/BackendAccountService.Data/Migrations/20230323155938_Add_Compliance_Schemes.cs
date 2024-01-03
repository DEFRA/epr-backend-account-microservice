using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Compliance_Schemes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplianceSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceSchemes_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SelectedSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationConnectionId = table.Column<int>(type: "int", nullable: false),
                    ComplianceSchemeId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectedSchemes_ComplianceSchemes_ComplianceSchemeId",
                        column: x => x.ComplianceSchemeId,
                        principalTable: "ComplianceSchemes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SelectedSchemes_OrganisationsConnections_OrganisationConnectionId",
                        column: x => x.OrganisationConnectionId,
                        principalTable: "OrganisationsConnections",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "InterOrganisationRoles",
                columns: new[] { "Id", "Name" },
                values: new object[] { 0, "Not Set" });

            migrationBuilder.InsertData(
                table: "InterOrganisationRoles",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Producer" });

            migrationBuilder.InsertData(
                table: "InterOrganisationRoles",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Compliance Scheme" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemes_OrganisationId",
                table: "ComplianceSchemes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedSchemes_ComplianceSchemeId",
                table: "SelectedSchemes",
                column: "ComplianceSchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedSchemes_OrganisationConnectionId",
                table: "SelectedSchemes",
                column: "OrganisationConnectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelectedSchemes");

            migrationBuilder.DropTable(
                name: "ComplianceSchemes");

            migrationBuilder.DeleteData(
                table: "InterOrganisationRoles",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "InterOrganisationRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InterOrganisationRoles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
