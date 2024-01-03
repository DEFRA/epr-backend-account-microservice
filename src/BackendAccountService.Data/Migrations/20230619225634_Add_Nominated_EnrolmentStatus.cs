using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class Add_Nominated_EnrolmentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegatedPersonEnrolments_Enrolments_InviterEnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_DelegatedPersonEnrolments_EnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_DelegatedPersonEnrolments_InviterEnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.RenameColumn(
                name: "InviterEnrolmentId",
                table: "DelegatedPersonEnrolments",
                newName: "NominatorEnrolmentId");

            migrationBuilder.RenameColumn(
                name: "InviterDeclarationTime",
                table: "DelegatedPersonEnrolments",
                newName: "NominatorDeclarationTime");

            migrationBuilder.RenameColumn(
                name: "InviterDeclaration",
                table: "DelegatedPersonEnrolments",
                newName: "NominatorDeclaration");

            migrationBuilder.RenameColumn(
                name: "InviteeDeclarationTime",
                table: "DelegatedPersonEnrolments",
                newName: "NomineeDeclarationTime");

            migrationBuilder.RenameColumn(
                name: "InviteeDeclaration",
                table: "DelegatedPersonEnrolments",
                newName: "NomineeDeclaration");

            migrationBuilder.InsertData(
                table: "EnrolmentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 7, "Nominated" });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_EnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "EnrolmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_NominatorEnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "NominatorEnrolmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegatedPersonEnrolments_Enrolments_NominatorEnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "NominatorEnrolmentId",
                principalTable: "Enrolments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegatedPersonEnrolments_Enrolments_NominatorEnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_DelegatedPersonEnrolments_EnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_DelegatedPersonEnrolments_NominatorEnrolmentId",
                table: "DelegatedPersonEnrolments");

            migrationBuilder.DeleteData(
                table: "EnrolmentStatuses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.RenameColumn(
                name: "NomineeDeclarationTime",
                table: "DelegatedPersonEnrolments",
                newName: "InviteeDeclarationTime");

            migrationBuilder.RenameColumn(
                name: "NomineeDeclaration",
                table: "DelegatedPersonEnrolments",
                newName: "InviteeDeclaration");

            migrationBuilder.RenameColumn(
                name: "NominatorEnrolmentId",
                table: "DelegatedPersonEnrolments",
                newName: "InviterEnrolmentId");

            migrationBuilder.RenameColumn(
                name: "NominatorDeclarationTime",
                table: "DelegatedPersonEnrolments",
                newName: "InviterDeclarationTime");

            migrationBuilder.RenameColumn(
                name: "NominatorDeclaration",
                table: "DelegatedPersonEnrolments",
                newName: "InviterDeclaration");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_EnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "EnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedPersonEnrolments_InviterEnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "InviterEnrolmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DelegatedPersonEnrolments_Enrolments_InviterEnrolmentId",
                table: "DelegatedPersonEnrolments",
                column: "InviterEnrolmentId",
                principalTable: "Enrolments",
                principalColumn: "Id");
        }
    }
}
