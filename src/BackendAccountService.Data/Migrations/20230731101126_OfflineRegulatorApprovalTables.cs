using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class OfflineRegulatorApprovalTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrolmentReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgName = table.Column<string>(name: "Org Name", type: "nvarchar(50)", nullable: true),
                    PCSorDirectproducer = table.Column<string>(name: "PCS or Direct producer?", type: "nvarchar(50)", nullable: true),
                    ComplianceSchemeName = table.Column<string>(name: "Compliance Scheme Name", type: "nvarchar(50)", nullable: true),
                    OrgType = table.Column<string>(name: "Org Type", type: "nvarchar(50)", nullable: true),
                    RequestType = table.Column<string>(name: "Request Type", type: "nvarchar(50)", nullable: true),
                    SubmittedDate = table.Column<DateTime>(name: "Submitted Date", type: "date", nullable: true),
                    OrganisationID = table.Column<string>(name: "Organisation ID", type: "nvarchar(50)", nullable: true),
                    CompaniesHouseNumber = table.Column<string>(name: "Companies House Number", type: "nvarchar(50)", nullable: true),
                    NationOfEnrolment = table.Column<string>(name: "Nation Of Enrolment", type: "nvarchar(50)", nullable: true),
                    Duedate = table.Column<DateTime>(name: "Due date", type: "date", nullable: true),
                    APFirstName = table.Column<string>(name: "AP First Name", type: "nvarchar(50)", nullable: true),
                    APLastName = table.Column<string>(name: "AP Last Name", type: "nvarchar(50)", nullable: true),
                    APPosition = table.Column<string>(name: "AP Position", type: "nvarchar(50)", nullable: true),
                    APContact = table.Column<string>(name: "AP Contact", type: "nvarchar(50)", nullable: true),
                    APEmail = table.Column<string>(name: "AP Email", type: "nvarchar(50)", nullable: true),
                    DPFirstName = table.Column<string>(name: "DP First Name", type: "nvarchar(50)", nullable: true),
                    DPLastName = table.Column<string>(name: "DP Last Name", type: "nvarchar(50)", nullable: true),
                    DPPosition = table.Column<string>(name: "DP Position", type: "nvarchar(50)", nullable: true),
                    DPNatureOfRelationship = table.Column<string>(name: "DP Nature Of Relationship", type: "nvarchar(50)", nullable: true),
                    DPContact = table.Column<string>(name: "DP Contact", type: "nvarchar(50)", nullable: true),
                    DPEmail = table.Column<string>(name: "DP Email", type: "nvarchar(50)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    ActionedBy = table.Column<string>(name: "Actioned By", type: "nvarchar(50)", nullable: true),
                    ActionedDate = table.Column<DateTime>(name: "Actioned Date", type: "date", nullable: true),
                    EnrolmentId = table.Column<string>(name: "Enrolment Id", type: "nvarchar(50)", nullable: true),
                    RegulatorStatus = table.Column<string>(name: "Regulator Status", type: "nvarchar(50)", nullable: true),
                    Date = table.Column<DateTime>(type: "date", nullable: true),
                    RegulatorUserName = table.Column<string>(name: "Regulator User Name", type: "nvarchar(50)", nullable: true),
                    RegulatorRejectionComments = table.Column<string>(name: "Regulator Rejection Comments", type: "nvarchar(200)", nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: true),
                    BatchNo = table.Column<int>(type: "int", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolmentReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfflineApprovalErrorLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Error_Number = table.Column<int>(type: "int", nullable: false),
                    Error_Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error_Line = table.Column<int>(type: "int", nullable: false),
                    Error_Procedure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error_Severity = table.Column<int>(type: "int", nullable: false),
                    Error_State = table.Column<int>(type: "int", nullable: false),
                    Error_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineApprovalErrorLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrolmentReport_ExternalId",
                table: "EnrolmentReport",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfflineApprovalErrorLog_ExternalId",
                table: "OfflineApprovalErrorLog",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrolmentReport");

            migrationBuilder.DropTable(
                name: "OfflineApprovalErrorLog");
        }
    }
}
