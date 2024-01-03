using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class add_regulator_comments_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegulatorCommentId",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferNationId",
                table: "Organisations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegulatorCommentId",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegulatorComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegulatorUserId = table.Column<int>(type: "int", nullable: false),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    RejectedComments = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransferComments = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OnHoldComments = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatorComments", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EnrolmentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "On Hold" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_RegulatorCommentId",
                table: "Persons",
                column: "RegulatorCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_TransferNationId",
                table: "Organisations",
                column: "TransferNationId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_RegulatorCommentId",
                table: "Enrolments",
                column: "RegulatorCommentId",
                unique: true,
                filter: "[RegulatorCommentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatorComments_ExternalId",
                table: "RegulatorComments",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_RegulatorComments_RegulatorCommentId",
                table: "Enrolments",
                column: "RegulatorCommentId",
                principalTable: "RegulatorComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_Nations_TransferNationId",
                table: "Organisations",
                column: "TransferNationId",
                principalTable: "Nations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_RegulatorComments_RegulatorCommentId",
                table: "Persons",
                column: "RegulatorCommentId",
                principalTable: "RegulatorComments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_RegulatorComments_RegulatorCommentId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_Nations_TransferNationId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_Persons_RegulatorComments_RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropTable(
                name: "RegulatorComments");

            migrationBuilder.DropIndex(
                name: "IX_Persons_RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_TransferNationId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_RegulatorCommentId",
                table: "Enrolments");

            migrationBuilder.DeleteData(
                table: "EnrolmentStatuses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "TransferNationId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegulatorCommentId",
                table: "Enrolments");
        }
    }
}
