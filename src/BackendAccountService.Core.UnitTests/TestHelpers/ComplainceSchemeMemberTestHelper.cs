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


        var relationshipType = new OrganisationRelationshipType
        {
            Id = 10007,
            Name = "Subsidiary"
        };

        setupContext.OrganisationRelationshipTypes.Add(relationshipType);

        var parentOrganisation = new Organisation
        {
            Name = "Parent Organisation",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("55555555-0000-0000-0000-000000000001"),
            IsComplianceScheme = false,
            ReferenceNumber = "199999",
            NationId = 1
        };
        setupContext.Organisations.Add(parentOrganisation);


        var childOrganisationWithData = new Organisation
        {
            Name = "Org Relationship Child 1",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("66666666-0000-0000-0000-000000000002"),
            IsComplianceScheme = false,
            ReferenceNumber = "200000",
            NationId = 1
        };

        var childOrganisationWithNulls = new Organisation
        {
            Name = "Org With Null Fields",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("77777777-0000-0000-0000-000000000003"),
            IsComplianceScheme = false,
            ReferenceNumber = "200001",
            NationId = 1
        };

        setupContext.Organisations.Add(childOrganisationWithData);
        setupContext.Organisations.Add(childOrganisationWithNulls);


        var relationshipWithData = new OrganisationRelationship
        {
            FirstOrganisationId = parentOrganisation.Id,
            SecondOrganisationId = childOrganisationWithData.Id,
            OrganisationRelationshipTypeId = relationshipType.Id,
            JoinerDate = DateTime.UtcNow
        };

        var relationshipWithNulls = new OrganisationRelationship
        {
            FirstOrganisationId = parentOrganisation.Id,
            SecondOrganisationId = childOrganisationWithNulls.Id,
            OrganisationRelationshipTypeId = relationshipType.Id,
            JoinerDate = null
        };

        setupContext.OrganisationRelationships.Add(relationshipWithData);
        setupContext.OrganisationRelationships.Add(relationshipWithNulls);
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}