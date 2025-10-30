using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationLeaverCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_ReportingTypes_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_ReportingTypeId",
                table: "OrganisationRelationships");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "JoinerDate",
                table: "OrganisationRelationships",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaverCodeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LeaverDate",
                table: "OrganisationRelationships",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganisationChangeReason",
                table: "OrganisationRelationships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaverCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ReasonsForLeaving = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaverCodes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LeaverCodes",
                columns: new[] { "Id", "Key", "ReasonsForLeaving" },
                values: new object[,]
                {
                    { 0, "", "Not Set" },
                    { 1, "A", "Administration/Receivership" },
                    { 2, "B", "Liquidation/dissolution" },
                    { 3, "C", "Dropped below turnover threshold" },
                    { 4, "D", "Dropped below tonnage threshold" },
                    { 5, "E", "Resignation (not incapacity related)" },
                    { 6, "F", "Scheme has terminated membership (not incapacity related)" },
                    { 7, "G", "Business closure (not incapacity related)" },
                    { 8, "H", "Bankruptcy" },
                    { 9, "I", "Merged with another company (not incapacity related)" },
                    { 10, "J", "Now a subsidiary of another company (not incapacity related)" },
                    { 11, "K", "Not ready to register by 15th April" },
                    { 12, "J", "No longer obligated (Not threshold related)" }
                });

            migrationBuilder.InsertData(
                table: "OrganisationRelationshipTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Child" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_LeaverCodeId",
                table: "OrganisationRelationships",
                column: "LeaverCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_LeaverCodes_LeaverCodeId",
                table: "OrganisationRelationships",
                column: "LeaverCodeId",
                principalTable: "LeaverCodes",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "ReportingTypeId",
                table: "OrganisationRelationships");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_LeaverCodes_LeaverCodeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "LeaverCodes");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationRelationships_LeaverCodeId",
                table: "OrganisationRelationships");

            migrationBuilder.DeleteData(
                table: "OrganisationRelationshipTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "LeaverCodeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "LeaverDate",
                table: "OrganisationRelationships");

            migrationBuilder.DropColumn(
                name: "OrganisationChangeReason",
                table: "OrganisationRelationships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinerDate",
                table: "OrganisationRelationships",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingTypeId",
                table: "OrganisationRelationships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRelationships_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_ReportingTypes_ReportingTypeId",
                table: "OrganisationRelationships",
                column: "ReportingTypeId",
                principalTable: "ReportingTypes",
                principalColumn: "Id");
        }
    }
}
