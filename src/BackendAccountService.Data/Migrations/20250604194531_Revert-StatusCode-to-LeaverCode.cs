using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevertStatusCodetoLeaverCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_StatusCodes_StatusCodeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "StatusCodes");

            migrationBuilder.RenameColumn(
                name: "StatusCodeId",
                table: "OrganisationRelationships",
                newName: "LeaverCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganisationRelationships_StatusCodeId",
                table: "OrganisationRelationships",
                newName: "IX_OrganisationRelationships_LeaverCodeId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_LeaverCodes_LeaverCodeId",
                table: "OrganisationRelationships",
                column: "LeaverCodeId",
                principalTable: "LeaverCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationRelationships_LeaverCodes_LeaverCodeId",
                table: "OrganisationRelationships");

            migrationBuilder.DropTable(
                name: "LeaverCodes");

            migrationBuilder.RenameColumn(
                name: "LeaverCodeId",
                table: "OrganisationRelationships",
                newName: "StatusCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganisationRelationships_LeaverCodeId",
                table: "OrganisationRelationships",
                newName: "IX_OrganisationRelationships_StatusCodeId");

            migrationBuilder.CreateTable(
                name: "StatusCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Reasons = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusCodes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StatusCodes",
                columns: new[] { "Id", "Key", "Reasons" },
                values: new object[,]
                {
                    { 0, "", "Not Set" },
                    { 1, "A", "Business is now small producer" },
                    { 2, "B", "Administration/Receivership/Liquidation/Dissolution" },
                    { 3, "C", "Confirmed Member - awaiting payment/membership forms" },
                    { 4, "D", "Confirmed Member - Awaiting packaging data" },
                    { 5, "E", "Confirmed Member - Awaiting reg file" },
                    { 6, "F", "Producer no longer obligated - Below Turnover" },
                    { 7, "G", "Producer no longer obligated - Below Packaging Threshold" },
                    { 8, "H", "Producer no longer obligated - No longer a producer" },
                    { 9, "I", "Small Producer Joined group but still reports for itself (2a)" },
                    { 10, "J", "Small Producer Joined group and parent reports on its behalf (2b)" },
                    { 11, "K", "Small Producer leaves group reported for itself (4a)" },
                    { 12, "L", "Small Producer leaves group parent reported for it (4b)" },
                    { 13, "M", "Large Producer left group but Starts to report for itself (3b, 5c)" },
                    { 14, "N", "Large Producer left group that reported for it (5d)" },
                    { 15, "O", "Large Producer Joined group and parent reports on its behalf (1b, 5b)" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationRelationships_StatusCodes_StatusCodeId",
                table: "OrganisationRelationships",
                column: "StatusCodeId",
                principalTable: "StatusCodes",
                principalColumn: "Id");
        }
    }
}
