using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_ReferenceName_Sequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "OrganisationReferenceNumber",
                startValue: 100001L);

            migrationBuilder.AlterColumn<string>(
                name: "Town",
                table: "Organisations",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "Organisations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValueSql: "NEXT VALUE FOR OrganisationReferenceNumber",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Postcode",
                table: "Organisations",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "OrganisationReferenceNumber");

            migrationBuilder.AlterColumn<string>(
                name: "Town",
                table: "Organisations",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "Organisations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldDefaultValueSql: "NEXT VALUE FOR OrganisationReferenceNumber");

            migrationBuilder.AlterColumn<string>(
                name: "Postcode",
                table: "Organisations",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
