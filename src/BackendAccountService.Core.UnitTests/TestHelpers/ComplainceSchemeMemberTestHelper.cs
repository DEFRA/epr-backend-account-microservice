namespace BackendAccountService.Core.UnitTests.TestHelpers;

using Data.Entities;
using Data.Infrastructure;
using FluentAssertions.Equivalency;

public static class ComplainceSchemeMemberTestHelper
{
    public static void SetUpDatabase(AccountsDbContext setupContext)
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        
        var complianceSchemeOrganisation1 = new Organisation
        {
            Name = "Compliance scheme 1",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123456",
            NationId = 1
        };
        var complianceSchemeOrganisation2 = new Organisation
        {
            Name = "Compliance scheme 2",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000002"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123457"
        };
        
        var complianceSchemeOrganisation3 = new Organisation
        {
            Name = "Compliance scheme 2",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000003"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123458"
        };

        setupContext.Organisations.Add(complianceSchemeOrganisation1);
        setupContext.Organisations.Add(complianceSchemeOrganisation2);
        setupContext.Organisations.Add(complianceSchemeOrganisation3);

        var complianceScheme1 = new ComplianceScheme
        {
            Name = "Compliance scheme 1",
            ExternalId = new Guid("22222222-0000-0000-0000-000000000000"),
            CompaniesHouseNumber = "CS123456"
        };
        var complianceScheme2 = new ComplianceScheme
        {
            Name = "Compliance scheme 1",
            ExternalId = new Guid("22222222-0000-0000-0000-000000000001"),
            CompaniesHouseNumber = "CS123458"
        };
        setupContext.ComplianceSchemes.Add(complianceScheme1);
        setupContext.ComplianceSchemes.Add(complianceScheme2);

        for (int x = 0; x < 200; x++)
        {
            var member = new Organisation
            {
                Name = $"Member {x}",
                OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                ExternalId =Guid.NewGuid(),
                IsComplianceScheme = false,
                ReferenceNumber = (200000 - x).ToString()
            };
            setupContext.Organisations.Add(member);

            var organisationConnection = new OrganisationsConnection
            {
                FromOrganisation = member,
                FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
                ToOrganisation = complianceSchemeOrganisation1,
                ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme,
                ExternalId = new Guid("33333333-0000-0000-0000-000000000001")
            };
            setupContext.OrganisationsConnections.Add(organisationConnection);

            var selectedScheme1 = new SelectedScheme
            {
                OrganisationConnection = organisationConnection,
                ComplianceScheme  = complianceScheme1
            };
            setupContext.SelectedSchemes.Add(selectedScheme1);
        }
        
        
        var member2 = new Organisation
        {
            Name = $"Organisation Name",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = Guid.NewGuid(),
            IsComplianceScheme = false,
            ReferenceNumber = "3000000",
            NationId = 1
        };
        setupContext.Organisations.Add(member2);

        var organisationConnection2 = new OrganisationsConnection
        {
            FromOrganisation = member2,
            FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
            ToOrganisation = complianceSchemeOrganisation1,
            ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme,
            ExternalId = new Guid("33333333-0000-0000-0000-000000000001")
        };
        setupContext.OrganisationsConnections.Add(organisationConnection2);

        var selectedScheme2 = new SelectedScheme
        {
            OrganisationConnection = organisationConnection2,
            ComplianceScheme  = complianceScheme1,
            ExternalId = new Guid("44444444-0000-0000-0000-000000000001")
        };
        setupContext.SelectedSchemes.Add(selectedScheme2);
        
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}