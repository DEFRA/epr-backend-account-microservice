using BackendAccountService.Data.DbConstants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_NewCompSchemes_Avask_WastepakNI : Migration
    {
        private const string WASTEPACK_COMPLIANCE_SCHEME_NAME = "Wastepack Northern Ireland (NIEA)";
        private const string WASTEPACK_COMPLIANCE_SCHEME_COMPANIES_HOUSE = "NI044560";
        private const int WASTERPACK_NATIONID = Nation.NorthernIreland;

        private const string AVASK_COMPLIANCE_SCHEME_NAME = "Avask";
        private const string AVASK_COMPLIANCE_SCHEME_COMPANIES_HOUSE = "08334523";
        private const int AVASK_NATIONID = Nation.England;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: ["Name", "CompaniesHouseNumber", "NationId"],
                values: new object[,]
                {
                    { WASTEPACK_COMPLIANCE_SCHEME_NAME, WASTEPACK_COMPLIANCE_SCHEME_COMPANIES_HOUSE, WASTERPACK_NATIONID }
                });

            migrationBuilder.InsertData(
                table: "ComplianceSchemes",
                columns: ["Name", "CompaniesHouseNumber", "NationId"],
                values: new object[,]
                {
                    { AVASK_COMPLIANCE_SCHEME_NAME, AVASK_COMPLIANCE_SCHEME_COMPANIES_HOUSE, AVASK_NATIONID }
                });

            string commandText = $"UPDATE Organisations SET IsComplianceScheme=1 WHERE CompaniesHouseNumber='{AVASK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND IsDeleted=0";
            migrationBuilder.Sql(commandText);
            commandText = $"UPDATE Organisations SET IsComplianceScheme=1 WHERE CompaniesHouseNumber='{WASTEPACK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND IsDeleted=0";
            migrationBuilder.Sql(commandText);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string deleteCommand = $"DELETE FROM ComplianceSchemes " +
                $"WHERE Name='{WASTEPACK_COMPLIANCE_SCHEME_NAME}' AND CompaniesHouseNumber='{WASTEPACK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND NationId={WASTERPACK_NATIONID}";
            migrationBuilder.Sql(deleteCommand);

            deleteCommand = $"DELETE FROM ComplianceSchemes " +
                $"WHERE Name='{AVASK_COMPLIANCE_SCHEME_NAME}' AND CompaniesHouseNumber='{AVASK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND NationId={AVASK_NATIONID}";
            migrationBuilder.Sql(deleteCommand);

            string commandText = $"UPDATE Organisations SET IsComplianceScheme=0 WHERE CompaniesHouseNumber='{AVASK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND IsDeleted=0";
            migrationBuilder.Sql(commandText);
            commandText = $"UPDATE Organisations SET IsComplianceScheme=0 WHERE CompaniesHouseNumber='{WASTEPACK_COMPLIANCE_SCHEME_COMPANIES_HOUSE}' AND IsDeleted=0";
            migrationBuilder.Sql(commandText);
        }
    }
}

