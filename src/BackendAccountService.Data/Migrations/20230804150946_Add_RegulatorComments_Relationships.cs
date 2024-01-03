using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_RegulatorComments_Relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_RegulatorComments_RegulatorCommentId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Persons_RegulatorComments_RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_RegulatorCommentId",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "RegulatorCommentId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "RegulatorCommentId",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "RegulatorUserId",
                table: "RegulatorComments",
                newName: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatorComments_EnrolmentId",
                table: "RegulatorComments",
                column: "EnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatorComments_PersonId",
                table: "RegulatorComments",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatorComments_Enrolments_EnrolmentId",
                table: "RegulatorComments",
                column: "EnrolmentId",
                principalTable: "Enrolments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatorComments_Persons_PersonId",
                table: "RegulatorComments",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegulatorComments_Enrolments_EnrolmentId",
                table: "RegulatorComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatorComments_Persons_PersonId",
                table: "RegulatorComments");

            migrationBuilder.DropIndex(
                name: "IX_RegulatorComments_EnrolmentId",
                table: "RegulatorComments");

            migrationBuilder.DropIndex(
                name: "IX_RegulatorComments_PersonId",
                table: "RegulatorComments");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "RegulatorComments",
                newName: "RegulatorUserId");

            migrationBuilder.AddColumn<int>(
                name: "RegulatorCommentId",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegulatorCommentId",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_RegulatorCommentId",
                table: "Persons",
                column: "RegulatorCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_RegulatorCommentId",
                table: "Enrolments",
                column: "RegulatorCommentId",
                unique: true,
                filter: "[RegulatorCommentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_RegulatorComments_RegulatorCommentId",
                table: "Enrolments",
                column: "RegulatorCommentId",
                principalTable: "RegulatorComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_RegulatorComments_RegulatorCommentId",
                table: "Persons",
                column: "RegulatorCommentId",
                principalTable: "RegulatorComments",
                principalColumn: "Id");
        }
    }
}
