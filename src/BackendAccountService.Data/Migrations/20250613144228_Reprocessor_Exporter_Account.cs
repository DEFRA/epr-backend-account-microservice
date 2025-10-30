using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reprocessor_Exporter_Account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationToPartnerRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<int>(type: "int", nullable: false),
                    PartnerRoleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationToPartnerRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationToPartnerRoles_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganisationToPartnerRoles_PartnerRoles_PartnerRoleId",
                        column: x => x.PartnerRoleId,
                        principalTable: "PartnerRoles",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PartnerRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Individual Partner" },
                    { 2, "Corporate Partner" }
                });

            migrationBuilder.InsertData(
                table: "PersonInOrganisationRoles",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Member" });

            migrationBuilder.InsertData(
                table: "ProducerTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 6, "Limited partnership" },
                    { 7, "Limited Liability partnership" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "Key", "Name" },
                values: new object[] { 4, "Extended Producer Responsibility For Packaging: Reprocessors And Exporters", "ReprocessorExporter", "EPR for packaging: reprocessors and exporters" });

            migrationBuilder.InsertData(
                table: "ServiceRoles",
                columns: new[] { "Id", "Description", "Key", "Name", "ServiceId" },
                values: new object[,]
                {
                    { 8, null, "Re-Ex.ApprovedPerson", "Approved Person", 4 },
                    { 9, null, "Re-Ex.DelegatedPerson", "Delegated Person", 4 },
                    { 10, null, "Re-Ex.BasicUser", "Basic User", 4 },
                    { 11, null, "Re-Ex.AdminUser", "Admin User", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationToPartnerRoles_OrganisationId",
                table: "OrganisationToPartnerRoles",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationToPartnerRoles_PartnerRoleId",
                table: "OrganisationToPartnerRoles",
                column: "PartnerRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationToPartnerRoles");

            migrationBuilder.DropTable(
                name: "PartnerRoles");

            migrationBuilder.DeleteData(
                table: "PersonInOrganisationRoles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProducerTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ProducerTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ServiceRoles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
