using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class CodeStatusConfigData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CodeStatusConfigId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodeClassificationLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodeClass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GroupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeClassificationLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ScenarioCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ObligationFlag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeStatusConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    LegacyCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClassificationId = table.Column<int>(type: "int", nullable: true),
                    RequiresJoinerDate = table.Column<bool>(type: "bit", nullable: true),
                    RequiresLeaverDate = table.Column<bool>(type: "bit", nullable: true),
                    RequiresRegType = table.Column<bool>(type: "bit", nullable: true),
                    MatchType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MappedOldCodes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeStatusConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeStatusConfigs_CodeClassificationLookups_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "CodeClassificationLookups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CodeScenarioMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodeStatusConfigId = table.Column<int>(type: "int", nullable: false),
                    ScenarioReferenceId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeScenarioMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeScenarioMapping_CodeStatusConfigs_CodeStatusConfigId",
                        column: x => x.CodeStatusConfigId,
                        principalTable: "CodeStatusConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CodeScenarioMapping_ScenarioReferences_ScenarioReferenceId",
                        column: x => x.ScenarioReferenceId,
                        principalTable: "ScenarioReferences",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "CodeClassificationLookups",
                columns: new[] { "Id", "CodeClass", "Description", "GroupType" },
                values: new object[,]
                {
                    { 0, "Not Set", "Not Set", "Not Set" },
                    { 1, "Joiner", "Joiner scenario", "Entry" },
                    { 2, "Leaver", "Leaver scenario", "Exit" }
                });

            migrationBuilder.UpdateData(
                table: "LeaverCodes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Key",
                value: "L");

            migrationBuilder.InsertData(
                table: "ScenarioReferences",
                columns: new[] { "Id", "Active", "Description", "ObligationFlag", "ScenarioCode" },
                values: new object[,]
                {
                    { 0, true, "Not Set", "Not Set", "Not Set" },
                    { 1, true, "Threshold producer joins a group", "Obligated", "1b" },
                    { 2, true, "Threshold producer joins a group", "Obligated", "5b" },
                    { 3, true, "Below-threshold producer joins group (% obligation)", "Obligated", "2a" },
                    { 4, true, "Below-threshold producer joins group (% obligation)", "Obligated", "2b" },
                    { 5, true, "Meets thresholds but group responsible for obligation", "Obligated", "3b" },
                    { 6, true, "Leaves group, holding company responsible", "Obligated", "4a" },
                    { 7, true, "Leaves group, holding company responsible", "Obligated", "4b" },
                    { 8, true, "Joined group mid-year, not obligated", "Not Obligated", "5d" },
                    { 9, true, "Left group mid-year, HC still responsible", "Obligated", "5d" },
                    { 10, true, "Joined group, not obligated", "Not Obligated", "5c" },
                    { 11, true, "Left group, HC still responsible", "Obligated", "5c" },
                    { 12, true, "Producer no longer obligated - insolvency or ceased function", "Not Obligated - HC", "none" },
                    { 13, true, "Resigned from compliance scheme", "Not Obligated - CS", "none" },
                    { 14, true, "Compliance scheme terminated membership", "Not Obligated - CS", "none" },
                    { 16, true, "Merged with another company - non-incapacity, CS only", "Not Obligated - CS", "none" },
                    { 17, true, "Producer data changed (e.g. acquisition of insolvent entity)", "Obligated", "none" },
                    { 18, true, "Became a producer mid-year - all scenarios", "Obligated", "all" }
                });

            migrationBuilder.InsertData(
                table: "CodeStatusConfigs",
                columns: new[] { "Id", "ClassificationId", "Code", "Description", "Enabled", "LegacyCode", "MappedOldCodes", "MatchType", "RequiresJoinerDate", "RequiresLeaverDate", "RequiresRegType" },
                values: new object[,]
                {
                    { 1, 1, "01", "Producer who previously met thresholds has joined a group.", true, "A", "None", "Direct", false, false, true },
                    { 2, 1, "02", "Producer who did not previously meet thresholds has joined a group (% obligation).", true, "B", "None", "Direct", true, false, true },
                    { 3, 1, "03", "Producer who did not previously meet thresholds has joined a group (% obligation).", true, "C", "None", "Direct", true, false, true },
                    { 4, 2, "04", "Producer left group; HC responsible for obligations due to MYC.", true, "D", "C,D", "Direct", false, true, true },
                    { 5, 2, "05", "Producer left group; HC responsible for obligations due to MYC.", true, "E", "C,D", "Direct", false, true, true },
                    { 6, 2, "06", "Producer meets thresholds; HC still responsible post-MYC.", true, "F", "None", "Direct", false, true, true },
                    { 7, 1, "07", "Producer joined group; HC not responsible due to MYC.", true, "G", "None", "Direct", false, false, true },
                    { 8, 2, "08", "Producer left group; HC still responsible due to MYC.", true, "H", "None", "Direct", false, true, true },
                    { 9, 1, "09", "Producer joined group; not obligated due to MYC.", true, "I", "None", "Direct", false, false, true },
                    { 10, 2, "10", "Producer left group; HC still responsible due to MYC.", true, "J", "J", "Direct", false, true, true },
                    { 11, 2, "11", "No longer obligated - insolvency event.", true, "K", "A,B,H", "ManualReview", false, true, true },
                    { 12, 2, "12", "Ceased performing producer function.", true, "L", "L", "ManualReview", false, true, true },
                    { 13, 2, "13", "Producer resigned from compliance scheme.", true, "M", "E", "ManualReview", false, true, true },
                    { 14, 2, "14", "CS terminated producer’s membership.", true, "N", "F", "ManualReview", false, true, true },
                    { 15, 1, "15", "Became a producer due to mid-year change.", true, "O", "None", "ManualReview", false, false, true },
                    { 16, 2, "16", "Merged with another company - not incapacity related.", true, "P", "I", "ManualReview", false, true, true },
                    { 17, 1, "17", "Producer data changed due to MYC (e.g. insolvent acquisition).", true, "Q", "A,B,H", "ManualReview", false, false, true }
                });

            migrationBuilder.InsertData(
                table: "CodeScenarioMapping",
                columns: new[] { "Id", "Active", "CodeStatusConfigId", "ScenarioReferenceId" },
                values: new object[,]
                {
                    { 1, true, 1, 1 },
                    { 2, true, 1, 2 },
                    { 3, true, 2, 3 },
                    { 4, true, 3, 4 },
                    { 5, true, 4, 6 },
                    { 6, true, 5, 7 },
                    { 7, true, 6, 5 },
                    { 8, true, 7, 8 },
                    { 9, true, 8, 9 },
                    { 10, true, 9, 10 },
                    { 11, true, 10, 11 },
                    { 12, true, 11, 12 },
                    { 13, true, 12, 12 },
                    { 14, true, 13, 13 },
                    { 15, true, 14, 14 },
                    { 16, true, 15, 18 },
                    { 17, true, 16, 16 },
                    { 18, true, 17, 17 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_CodeStatusConfigId",
                table: "OrganisationRelationships",
                column: "CodeStatusConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeClassificationLookups_ExternalId",
                table: "CodeClassificationLookups",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeScenarioMapping_CodeStatusConfigId",
                table: "CodeScenarioMapping",
                column: "CodeStatusConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeScenarioMapping_ExternalId",
                table: "CodeScenarioMapping",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeScenarioMapping_ScenarioReferenceId",
                table: "CodeScenarioMapping",
                column: "ScenarioReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeStatusConfigs_ClassificationId",
                table: "CodeStatusConfigs",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeStatusConfigs_ExternalId",
                table: "CodeStatusConfigs",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioReferences_ExternalId",
                table: "ScenarioReferences",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_CodeStatusConfigs_CodeStatusConfigId",
                table: "OrganisationRelationships",
                column: "CodeStatusConfigId",
                principalTable: "CodeStatusConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_CodeStatusConfigs_CodeStatusConfigId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "CodeScenarioMapping");

            migrationBuilder.DropTable(
                name: "CodeStatusConfigs");

            migrationBuilder.DropTable(
                name: "ScenarioReferences");

            migrationBuilder.DropTable(
                name: "CodeClassificationLookups");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_CodeStatusConfigId",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "CodeStatusConfigId",
                table: "OrganisationRelationships");

            migrationBuilder.UpdateData(
                table: "LeaverCodes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Key",
                value: "J");
        }
    }
}
