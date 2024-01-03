using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrolmentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterOrganisationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterOrganisationRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterPersonRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterPersonRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationToPersonRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationToPersonRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonInOrganisationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonInOrganisationRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalIdpId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "External Provider Identity ID"),
                    ExternalIdpUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "External Provider Identity User ID"),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationTypeId = table.Column<int>(type: "int", nullable: false),
                    CompaniesHouseNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TradingName = table.Column<string>(type: "nvarchar(170)", maxLength: 170, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SubBuildingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BuildingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Locality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DependentLocality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Town = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    County = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ValidatedWithCompaniesHouse = table.Column<bool>(type: "bit", nullable: false),
                    IsComplianceScheme = table.Column<bool>(type: "bit", nullable: false),
                    NationId = table.Column<int>(type: "int", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organisations_Nations_NationId",
                        column: x => x.NationId,
                        principalTable: "Nations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Organisations_OrganisationTypes_OrganisationTypeId",
                        column: x => x.OrganisationTypeId,
                        principalTable: "OrganisationTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRoles_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganisationsConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromOrganisationId = table.Column<int>(type: "int", nullable: false),
                    FromOrganisationRoleId = table.Column<int>(type: "int", nullable: false),
                    ToOrganisationId = table.Column<int>(type: "int", nullable: false),
                    ToOrganisationRoleId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationsConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationsConnections_InterOrganisationRoles_FromOrganisationRoleId",
                        column: x => x.FromOrganisationRoleId,
                        principalTable: "InterOrganisationRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationsConnections_InterOrganisationRoles_ToOrganisationRoleId",
                        column: x => x.ToOrganisationRoleId,
                        principalTable: "InterOrganisationRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationsConnections_Organisations_FromOrganisationId",
                        column: x => x.FromOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationsConnections_Organisations_ToOrganisationId",
                        column: x => x.ToOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonOrganisationConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganisationId = table.Column<int>(type: "int", nullable: false),
                    OrganisationRoleId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PersonRoleId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonOrganisationConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnections_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnections_OrganisationToPersonRoles_OrganisationRoleId",
                        column: x => x.OrganisationRoleId,
                        principalTable: "OrganisationToPersonRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnections_PersonInOrganisationRoles_PersonRoleId",
                        column: x => x.PersonRoleId,
                        principalTable: "PersonInOrganisationRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonOrganisationConnections_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonsConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromPersonId = table.Column<int>(type: "int", nullable: false),
                    FromPersonRoleId = table.Column<int>(type: "int", nullable: false),
                    ToPersonId = table.Column<int>(type: "int", nullable: false),
                    ToPersonRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonsConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonsConnections_InterPersonRoles_FromPersonRoleId",
                        column: x => x.FromPersonRoleId,
                        principalTable: "InterPersonRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonsConnections_InterPersonRoles_ToPersonRoleId",
                        column: x => x.ToPersonRoleId,
                        principalTable: "InterPersonRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonsConnections_Persons_FromPersonId",
                        column: x => x.FromPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonsConnections_Persons_ToPersonId",
                        column: x => x.ToPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConnectionId = table.Column<int>(type: "int", nullable: false),
                    ServiceRoleId = table.Column<int>(type: "int", nullable: false),
                    EnrolmentStatusId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrolments_EnrolmentStatuses_EnrolmentStatusId",
                        column: x => x.EnrolmentStatusId,
                        principalTable: "EnrolmentStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enrolments_PersonOrganisationConnections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "PersonOrganisationConnections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enrolments_ServiceRoles_ServiceRoleId",
                        column: x => x.ServiceRoleId,
                        principalTable: "ServiceRoles",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "EnrolmentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Enrolled" },
                    { 2, "Pending" },
                    { 3, "Approved" },
                    { 4, "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "Nations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "England" },
                    { 2, "Northern Ireland" },
                    { 3, "Scotland" },
                    { 4, "Wales" }
                });

            migrationBuilder.InsertData(
                table: "OrganisationToPersonRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Employer" }
                });

            migrationBuilder.InsertData(
                table: "OrganisationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Limited Company" },
                    { 2, "Sole Trader" }
                });

            migrationBuilder.InsertData(
                table: "PersonInOrganisationRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Admin" },
                    { 2, "Employee" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "Key", "Name" },
                values: new object[] { 1, "Extended Producer Responsibility - Packaging", "Packaging", "EPR Packaging" });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 1, null, "Packaging.ApprovedPerson", "Approved Person", 1 });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 2, null, "Packaging.DelegatedPerson", "Delegated Person", 1 });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[] { 3, null, "Packaging.BasicUser", "Basic User", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ConnectionId",
                table: "Enrolments",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EnrolmentStatusId",
                table: "Enrolments",
                column: "EnrolmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ServiceRoleId",
                table: "Enrolments",
                column: "ServiceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_NationId",
                table: "Organisations",
                column: "NationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_OrganisationTypeId",
                table: "Organisations",
                column: "OrganisationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationsConnections_FromOrganisationId",
                table: "OrganisationsConnections",
                column: "FromOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationsConnections_FromOrganisationRoleId",
                table: "OrganisationsConnections",
                column: "FromOrganisationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationsConnections_ToOrganisationId",
                table: "OrganisationsConnections",
                column: "ToOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationsConnections_ToOrganisationRoleId",
                table: "OrganisationsConnections",
                column: "ToOrganisationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnections_OrganisationId",
                table: "PersonOrganisationConnections",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnections_OrganisationRoleId",
                table: "PersonOrganisationConnections",
                column: "OrganisationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnections_PersonId",
                table: "PersonOrganisationConnections",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOrganisationConnections_PersonRoleId",
                table: "PersonOrganisationConnections",
                column: "PersonRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserId",
                table: "Persons",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsConnections_FromPersonId",
                table: "PersonsConnections",
                column: "FromPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsConnections_FromPersonRoleId",
                table: "PersonsConnections",
                column: "FromPersonRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsConnections_ToPersonId",
                table: "PersonsConnections",
                column: "ToPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsConnections_ToPersonRoleId",
                table: "PersonsConnections",
                column: "ToPersonRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRoles_ServiceId",
                table: "ServiceRoles",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "OrganisationsConnections");

            migrationBuilder.DropTable(
                name: "PersonsConnections");

            migrationBuilder.DropTable(
                name: "EnrolmentStatuses");

            migrationBuilder.DropTable(
                name: "PersonOrganisationConnections");

            migrationBuilder.DropTable(
                name: "ServiceRoles");

            migrationBuilder.DropTable(
                name: "InterOrganisationRoles");

            migrationBuilder.DropTable(
                name: "InterPersonRoles");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationToPersonRoles");

            migrationBuilder.DropTable(
                name: "PersonInOrganisationRoles");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Nations");

            migrationBuilder.DropTable(
                name: "OrganisationTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
