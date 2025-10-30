namespace BackendAccountService.Core.UnitTests.TestHelpers;

using Data.Entities;
using Data.Infrastructure;

public static class ComplainceSchemeTestHelper
{
    public static void SetUpDatabase(AccountsDbContext setupContext)
    {
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        
        var csOrg1 = new Organisation
        {
            Name = "Org 1",
            Id = 10000,
            OrganisationTypeId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000001"),
            CompaniesHouseNumber = "111111",
            IsComplianceScheme = true,
            NationId = Data.DbConstants.Nation.England,
            ReferenceNumber = "199999",
        };
        var csOrg2 = new Organisation
        {
            Name = "Org 2",
            Id = 10001,
            OrganisationTypeId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000002"),
            CompaniesHouseNumber = "222222",
            IsComplianceScheme = true
        };
        var prodOrg1 = new Organisation
        {
            Name = "Org 3",
            OrganisationTypeId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000003"),
            CompaniesHouseNumber = "444444",
            IsComplianceScheme = false
        };
        var prodOrg2 = new Organisation
        {
            Name = "Org 4",
            OrganisationTypeId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000004"),
            CompaniesHouseNumber = "555555",
            IsComplianceScheme = false
        };
        setupContext.Organisations.Add(csOrg1);
        setupContext.Organisations.Add(csOrg2);
        setupContext.Organisations.Add(prodOrg1);
        setupContext.Organisations.Add(prodOrg2);

        var member = new Organisation
        {
            Name = "Member",
            ProducerTypeId = 1,
            ExternalId =Guid.NewGuid(),
            IsComplianceScheme = true,
            ReferenceNumber = "8924785",
            NationId = Data.DbConstants.Nation.England,
            
        };
        setupContext.Organisations.Add(member);
        var OrgConnection = new OrganisationsConnection
        {
            FromOrganisation = csOrg1,
            FromOrganisationRoleId = 1,
            ToOrganisation = csOrg2,
            ToOrganisationRoleId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000011")
        };
        var OrgConnection2 = new OrganisationsConnection
        {
            FromOrganisationId = 2,
            FromOrganisationRoleId = 1,
            ToOrganisationId = 2,
            ToOrganisationRoleId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000012")
        };
        var OrgConnection3 = new OrganisationsConnection
        {
            FromOrganisation = prodOrg1,
            FromOrganisationRoleId = 1,
            ToOrganisation = csOrg2,
            ToOrganisationRoleId = 2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000013")
        };
        var OrgConnection4 = new OrganisationsConnection
        {
            FromOrganisation = prodOrg2,
            FromOrganisationRoleId = 1,
            ToOrganisation = csOrg2,
            ToOrganisationRoleId = 2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000013")
        };
        
        var OrgConnection5 = new OrganisationsConnection
        {
            FromOrganisation = member,
            FromOrganisationRoleId = 1,
            ToOrganisation = csOrg1,
            ToOrganisationRoleId = 2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000014")
        };
        
        setupContext.OrganisationsConnections.Add(OrgConnection);
        setupContext.OrganisationsConnections.Add(OrgConnection2);
        setupContext.OrganisationsConnections.Add(OrgConnection3);
        setupContext.OrganisationsConnections.Add(OrgConnection4);
        setupContext.OrganisationsConnections.Add(OrgConnection5);

        var complianceScheme1 = new ComplianceScheme
        {
            Name = "Compliance Scheme 1",
            CompaniesHouseNumber = csOrg1.CompaniesHouseNumber,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000021"),
            NationId = 1
        };
        var complianceScheme2 = new ComplianceScheme
        {
            Name = "Compliance Scheme 2",
            CompaniesHouseNumber = csOrg1.CompaniesHouseNumber,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000022"),
            NationId = 1
        };
        var complianceScheme3 = new ComplianceScheme
        {
            Name = "Compliance Scheme 3",
            CompaniesHouseNumber = csOrg2.CompaniesHouseNumber,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000023"),
            NationId = 1
        };
        var complianceScheme4 = new ComplianceScheme
        {
            Name = "Compliance Scheme 4",
            CompaniesHouseNumber = "333333",
            ExternalId = new Guid("00000000-0000-0000-0000-000000000024"),
            NationId = 1
        };
        setupContext.ComplianceSchemes.Add(complianceScheme1);
        setupContext.ComplianceSchemes.Add(complianceScheme2);
        setupContext.ComplianceSchemes.Add(complianceScheme3);
        setupContext.ComplianceSchemes.Add(complianceScheme4);

        var selectedScheme = new SelectedScheme
        {
            ComplianceSchemeId = 1,
            OrganisationConnectionId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000031")
        };
        var selectedScheme2 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000032")
        };
        var selectedScheme3 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000033")
        };
        var selectedScheme4 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection3,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000034")
        };
        var selectedScheme5 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection3,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000035")
        };
        var selectedScheme6 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection4,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000036")
        };
        var selectedScheme7 = new SelectedScheme
        {
            ComplianceScheme = complianceScheme1,
            OrganisationConnection = OrgConnection5,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000037")
        };
            
        setupContext.SelectedSchemes.Add(selectedScheme);
        setupContext.SelectedSchemes.Add(selectedScheme2);
        setupContext.SelectedSchemes.Add(selectedScheme3);
        setupContext.SelectedSchemes.Add(selectedScheme4);
        setupContext.SelectedSchemes.Add(selectedScheme5);
        setupContext.SelectedSchemes.Add(selectedScheme6);
        setupContext.SelectedSchemes.Add(selectedScheme7);

        var organisation10 = new Organisation
        {
            Id = 10100,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel100124",
            IsComplianceScheme = true,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Second Test Relationship Organisation2",
            SubBuildingName = "Sub building 1",
            BuildingName = "Building 1",
            BuildingNumber = "1",
            Street = "Street 1",
            Locality = "Locality 1",
            DependentLocality = "Dependent Locality 1",
            Town = "Town 1",
            County = "County 1",
            Postcode = "BT44 5QW",
            Country = "Country 1",
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000010"),
            ReferenceNumber = "1000010",
        };
        setupContext.Organisations.Add(organisation10);

        var relationship1 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10100,
            OrganisationRelationshipTypeId = 10007
        };
        setupContext.OrganisationRelationships.Add(relationship1);

        var relationship2 = new OrganisationRelationship
        {
            FirstOrganisationId = 10001,
            SecondOrganisationId = 10001,
            OrganisationRelationshipTypeId = 10007
        };
        setupContext.OrganisationRelationships.Add(relationship2);

        var relationshipType1 = new Data.Entities.OrganisationRelationshipType
        {
            Id = 10007,
            Name = "Parent"
        };
        setupContext.OrganisationRelationshipTypes.Add(relationshipType1);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);

    }
    
    public static Organisation AddOrganisation(this AccountsDbContext dbContext, Organisation organisation)
    {
        dbContext.Organisations.Add(organisation);
        dbContext.SaveChanges(Guid.Empty, Guid.Empty);
        return organisation;
    }

    public static class SummaryData
    {
        private const string CompaniesHouseNumber = "CS-OPERATOR";
        private static int _complianceSchemesCount = 0;
        private static int _producerOrganisationsCount = 0;
        
        public static Organisation AddOperatorOrganisation(AccountsDbContext dbContext, bool isDeleted = false)
        {
            var organisationId = Guid.NewGuid();
            
            var operatorOrganisation = new Organisation
            {
                Name = "Operator Organisation",
                OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                ExternalId = organisationId,
                CompaniesHouseNumber = CompaniesHouseNumber,
                IsComplianceScheme = true,
                IsDeleted = isDeleted
            };
            
            dbContext.Organisations.Add(operatorOrganisation);

            return operatorOrganisation;
        }

        public static ComplianceScheme AddComplianceScheme(AccountsDbContext dbContext, int nationId, bool isDeleted = false)
        {
            var complianceScheme = new ComplianceScheme
            {
                Name = $"Compliance Scheme {++_complianceSchemesCount}",
                CompaniesHouseNumber = CompaniesHouseNumber,
                ExternalId = Guid.NewGuid(),
                Nation = dbContext.Nations.FirstOrDefault(nation => nation.Id == nationId),
                IsDeleted = isDeleted
            };
            
            typeof(ComplianceScheme).BaseType
                .GetProperty("CreatedOn")
                .SetValue(complianceScheme, DateTimeOffset.Now);
 
            dbContext.ComplianceSchemes.Add(complianceScheme);

            return complianceScheme;
        }

        public static void AddSchemeMembers(AccountsDbContext dbContext, Organisation operatorOrganisation, ComplianceScheme complianceScheme, int membersCount)
        {
            for (int memberNo = 1; memberNo <= membersCount; ++memberNo)
            {
                var producerOrganisation = AddProducerOrganisation(dbContext);

                var producerOperatorConnection = AddProducerOperatorConnection(dbContext, producerOrganisation, operatorOrganisation);

                AddSelectedScheme(dbContext, complianceScheme, producerOperatorConnection);
            }
        }
        
        public static void AddDeletedSchemeMembers(AccountsDbContext dbContext, Organisation operatorOrganisation, ComplianceScheme complianceScheme, int deletedMembersCount)
        {
            for (int memberNo = 1; memberNo <= deletedMembersCount; ++memberNo)
            {
                var producerOrganisation = AddProducerOrganisation(dbContext);

                var producerOperatorConnection = AddProducerOperatorConnection(dbContext, producerOrganisation, operatorOrganisation);

                AddSelectedScheme(dbContext, complianceScheme, producerOperatorConnection, isDeleted: true);
            }
        }
        
        private static Organisation AddProducerOrganisation(AccountsDbContext dbContext, bool isDeleted = false)
        {
            var producerOrganisation = new Organisation
            {
                Name = $"Producer {++_producerOrganisationsCount}",
                OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                ExternalId = Guid.NewGuid(),
                CompaniesHouseNumber = $"Producer {_producerOrganisationsCount}",
                IsDeleted = isDeleted
            };
        
            dbContext.Organisations.Add(producerOrganisation);
            
            return producerOrganisation;
        }
        
        private static OrganisationsConnection AddProducerOperatorConnection(AccountsDbContext dbContext,
            Organisation producerOrganisation, Organisation operatorOrganisation, bool isDeleted = false)
        {
            var producerOperatorConnection = new OrganisationsConnection
            {
                FromOrganisation = producerOrganisation,
                FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
                ToOrganisation = operatorOrganisation,
                ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme,
                ExternalId = Guid.NewGuid(),
                IsDeleted = isDeleted
            };

            dbContext.OrganisationsConnections.Add(producerOperatorConnection);

            return producerOperatorConnection;
        }

        private static void AddSelectedScheme(AccountsDbContext dbContext, ComplianceScheme complianceScheme, OrganisationsConnection producerOperatorConnection, bool isDeleted = false)
        {
            var selectedScheme = new SelectedScheme
            {
                ComplianceScheme = complianceScheme,
                OrganisationConnection = producerOperatorConnection,
                ExternalId = Guid.NewGuid(),
                IsDeleted = isDeleted
            };

            var dataEntityType = typeof(SelectedScheme).BaseType;
            
            dataEntityType.GetProperty("CreatedOn").SetValue(selectedScheme, DateTimeOffset.Now);
            dataEntityType.GetProperty("LastUpdatedOn").SetValue(selectedScheme, DateTimeOffset.Now);
            
            dbContext.SelectedSchemes.Add(selectedScheme);
        }
    }
}