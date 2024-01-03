using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_ComplianceSchemeMemberRemoval_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplianceSchemeMemberRemovalAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeOrganisationId = table.Column<int>(type: "int", nullable: false),
                    MemberOrganisationId = table.Column<int>(type: "int", nullable: false),
                    ComplianceSchemeId = table.Column<int>(type: "int", nullable: false),
                    RemovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceSchemeMemberRemovalAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceSchemeMemberRemovalAuditLogs_ComplianceSchemes_ComplianceSchemeId",
                        column: x => x.ComplianceSchemeId,
                        principalTable: "ComplianceSchemes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceSchemeMemberRemovalAuditLogs_Organisations_MemberOrganisationId",
                        column: x => x.MemberOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceSchemeMemberRemovalAuditLogs_Organisations_SchemeOrganisationId",
                        column: x => x.SchemeOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceSchemeMemberRemovalReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequiresReason = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceSchemeMemberRemovalReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceSchemeMemberRemovalAuditLogsReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditLogId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceSchemeMemberRemovalAuditLogsReasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceSchemeMemberRemovalAuditLogsReasons_ComplianceSchemeMemberRemovalAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "ComplianceSchemeMemberRemovalAuditLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceSchemeMemberRemovalAuditLogsReasons_ComplianceSchemeMemberRemovalReasons_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "ComplianceSchemeMemberRemovalReasons",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ComplianceSchemeMemberRemovalReasons",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name", "RequiresReason" },
                values: new object[,]
                {
                    { 1, "A", 1, false, "It won't be able to give us its organisation details or packaging data by the deadline", false },
                    { 2, "B", 2, false, "The amount of packaging it handled in the threshold calculation year is less than or equal to 25 tonnes", false },
                    { 3, "C", 3, false, "It has ended its membership with this scheme", false },
                    { 4, "D", 4, false, "It has merged with another company", false },
                    { 5, "E", 5, false, "It is no longer in operation", true },
                    { 6, "F", 6, false, "It has gone into administration", false },
                    { 7, "G", 7, false, "It is now a subsidiary of another company", false },
                    { 8, "H", 8, false, "Its turnover in the last financial year before the relevant date is less than or equal to �1 million", false },
                    { 9, "I", 9, false, "We've ended its membership with this scheme", false },
                    { 10, "J", 10, false, "None of the above", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalAuditLogs_ComplianceSchemeId",
                table: "ComplianceSchemeMemberRemovalAuditLogs",
                column: "ComplianceSchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalAuditLogs_MemberOrganisationId",
                table: "ComplianceSchemeMemberRemovalAuditLogs",
                column: "MemberOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalAuditLogs_SchemeOrganisationId",
                table: "ComplianceSchemeMemberRemovalAuditLogs",
                column: "SchemeOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalAuditLogsReasons_AuditLogId",
                table: "ComplianceSchemeMemberRemovalAuditLogsReasons",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalAuditLogsReasons_ReasonId",
                table: "ComplianceSchemeMemberRemovalAuditLogsReasons",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceSchemeMemberRemovalReasons_Code",
                table: "ComplianceSchemeMemberRemovalReasons",
                column: "Code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceSchemeMemberRemovalAuditLogsReasons");

            migrationBuilder.DropTable(
                name: "ComplianceSchemeMemberRemovalAuditLogs");

            migrationBuilder.DropTable(
                name: "ComplianceSchemeMemberRemovalReasons");
        }
    }
}
