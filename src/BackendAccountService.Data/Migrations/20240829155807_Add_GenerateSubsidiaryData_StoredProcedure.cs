using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_GenerateSubsidiaryData_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly
                .GetManifestResourceNames()
                .Single(str => str.EndsWith("GenerateSubsidiaryData.sql"));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            var sql = reader.ReadToEnd();

            var exec = $"DECLARE @sql nvarchar(max) = '{sql.Replace("'", "''")}';\n" +
                "EXEC sp_executesql @statement = @sql;";

            migrationBuilder.Sql(exec);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "[dbo].DROP PROCEDURE IF EXISTS [GenerateSubsidiaryData]";
            migrationBuilder.Sql(sql);
        }
    }
}
