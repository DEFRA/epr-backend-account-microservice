using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_PersonOrganisationConnectionInvites_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonOrganisationConnectionInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InviteePersonId = table.Column<int>(type: "int", nullable: false),
                    OrganisationId = table.Column<int>(type: "int", nullable: false),
                    InvitedByUserId = table.Column<int>(type: "int", nullable: false),
                    InviteToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonOrganisationConnectionInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnectionInvites_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnectionInvites_Persons_InviteePersonId",
                        column: x => x.InviteePersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnectionInvites_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnectionInvites_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnectionInvites_ExternalId",
                table: "PersonOrganisationConnectionInvites",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnectionInvites_InvitedByUserId",
                table: "PersonOrganisationConnectionInvites",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnectionInvites_InviteePersonId",
                table: "PersonOrganisationConnectionInvites",
                column: "InviteePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnectionInvites_OrganisationId",
                table: "PersonOrganisationConnectionInvites",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnectionInvites_ServiceId",
                table: "PersonOrganisationConnectionInvites",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonOrganisationConnectionInvites");
        }
    }
}
