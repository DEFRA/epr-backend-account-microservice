using BackendAccountService.Data.DbConstants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAccountService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_System_User_And_Organisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string SystemUserEmail = ApplicationUser.System;

            var commandText = 
                $"""
                DECLARE @userId INT;
                DECLARE @personId INT;
                DECLARE @organisationId INT;
                DECLARE @systemUserEmail NVARCHAR(254) = '{SystemUserEmail}'

                INSERT INTO [dbo].[Users]
                    ([UserId]
                     ,[Email])
                 VALUES 
                    (NEWID()
                     ,@systemUserEmail);

                SELECT @userId = SCOPE_IDENTITY();

                INSERT INTO [dbo].[Persons]
                        ([FirstName]
                        ,[LastName]
                        ,[Email]
                        ,[Telephone]
                        ,[UserId])
                    VALUES
                        ('System'
                        ,'User'
                        ,'@systemUserEmail'
                        ,''
                        ,@userId);

                SELECT @personId = SCOPE_IDENTITY()

                INSERT INTO [dbo].[Organisations]
                        ([OrganisationTypeId]
                        ,[Name]
                        ,[ValidatedWithCompaniesHouse]
                        ,[IsComplianceScheme])
                    VALUES
                        (0
                         ,'System Organisation'
                         ,0
                         ,0);
                SELECT @organisationId = SCOPE_IDENTITY()

                INSERT INTO [dbo].[PersonOrganisationConnections]
                    ([JobTitle]
                     ,[OrganisationId]
                     ,[OrganisationRoleId]
                     ,[PersonId]
                     ,[PersonRoleId])
                VALUES
                    ('System user'
                     ,@organisationId
                     ,0
                     ,@personId
                    ,0);
                """;

            migrationBuilder.Sql(commandText);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not implemented because the added user and organisation will be used in foreign keys
        }
    }
}
