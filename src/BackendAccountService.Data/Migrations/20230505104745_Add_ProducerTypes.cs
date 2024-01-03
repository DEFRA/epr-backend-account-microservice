using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_ProducerTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProducerTypeId",
                table: "Organisations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProducerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerTypes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Companies House Company");

            migrationBuilder.UpdateData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Non Companies House Company");

            migrationBuilder.InsertData(
                table: "ProducerTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Set" },
                    { 1, "Partnership" },
                    { 2, "Unincorporated body" },
                    { 3, "Non-UK organisation" },
                    { 4, "Sole trader" },
                    { 5, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_ProducerTypeId",
                table: "Organisations",
                column: "ProducerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_ProducerTypes_ProducerTypeId",
                table: "Organisations",
                column: "ProducerTypeId",
                principalTable: "ProducerTypes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_ProducerTypes_ProducerTypeId",
                table: "Organisations");

            migrationBuilder.DropTable(
                name: "ProducerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_ProducerTypeId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ProducerTypeId",
                table: "Organisations");

            migrationBuilder.UpdateData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Limited Company");

            migrationBuilder.UpdateData(
                table: "OrganisationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Sole Trader");
        }
    }
}
