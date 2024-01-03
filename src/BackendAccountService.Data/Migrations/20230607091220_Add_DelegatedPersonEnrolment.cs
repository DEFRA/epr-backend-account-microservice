using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_DelegatedPersonEnrolment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DelegatedPersonEnrolments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    InviterEnrolmentId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsultancyName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ComplianceSchemeName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OtherOrganisationName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OtherRelationshipDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    InviterDeclaration = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InviterDeclarationTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InviteeDeclaration = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InviteeDeclarationTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatedPersonEnrolments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelegatedPersonEnrolments_Enrolments_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DelegatedPersonEnrolments_Enrolments_InviterEnrolmentId",
                        column: x => x.InviterEnrolmentId,
                        principalTable: "Enrolments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_EnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "EnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_InviterEnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "InviterEnrolmentId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DelegatedPersonEnrolments");
        }
    }
}
