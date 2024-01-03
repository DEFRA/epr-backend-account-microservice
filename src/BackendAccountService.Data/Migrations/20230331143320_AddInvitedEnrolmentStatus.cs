using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    public partial class AddInvitedEnrolmentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EnrolmentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "Invited" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EnrolmentStatuses",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
