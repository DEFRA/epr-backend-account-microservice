using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class OrganisationServiceSearchTests
{
    private AccountsDbContext _accountContext = null!;
    private OrganisationService _organisationService = null!;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _accountContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationService(
            _accountContext);
    }

    [TestMethod]
    [DataRow("Test organisation 1")]
    [DataRow("tEST oRganisation 1")]
    [DataRow("rEference numBEr 1")]
    [DataRow("Reference Number 1")]
    [DataRow(" Reference  Number  1    ")]
    public async Task WhenGetOrganisationsBySearchTermForProducerOrganisationRole_ThenReturnCorrectValues(string query)
    {
        var organisation = CreateOrganisation();
        organisation.Name = "Test organisation 1";
        organisation.ReferenceNumber = "ReferenceNumber1";

        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.AddAsync(organisation, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 1, 1);
        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForPage1_ThenReturnCorrectValues()
    {
        var query = "Test org";
        foreach (var orgName in new[] { "Test org 1", "Test org 2", "Test org 3" })
        {
            var organisation = CreateOrganisation();
            organisation.Name = orgName;
            var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);

            await _accountContext.AddAsync(organisation, default);
            await _accountContext.AddAsync(complianceScheme, default);
        }

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(3);
        results.Items.Should().Contain(item => item.OrganisationName == "Test org 1");
        results.Items.Should().Contain(item => item.OrganisationName == "Test org 2");
        results.Items.Should().Contain(item => item.OrganisationName == "Test org 3");
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForPage2_ThenReturnCorrectValues()
    {
        var query = "Test org";
        foreach (var orgName in new[] { "Test org 1", "Test org 2", "Test org 3", "Test org 4" })
        {
            var organisation = CreateOrganisation();
            organisation.Name = orgName;
            var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);

            await _accountContext.AddAsync(organisation, default);
            await _accountContext.AddAsync(complianceScheme, default);
        }

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 2);
        results.Items.Count.Should().Be(1);
        results.Items.Should().Contain(item => item.OrganisationName == "Test org 4");
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForOrganisationWhichIsDeleted_ThenReturnNone()
    {
        var organisation = CreateOrganisation();
        organisation.IsDeleted = true;

        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);
        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWithDifferentNationCode_ThenReturnNone()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);
        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.Scotland, 2, 1);
        results.Items.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForOrganisationWhichIsComplianceScheme_ThenReturnCorrectValues()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);

        organisation.IsComplianceScheme = true;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(1);

        AddAssert(results.Items[0], organisation);
    }
    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForOrganisationWhichIsOfADifferentCountryFromThatOfTheRegulator_ThenReturnCorrectValues()
    {
        // Arrange
        var nations = CreateNations();
        foreach (var nation in nations)
        {
            _accountContext.Add(nation);
        }

        var organisationOne = CreateOrganisation();
        _accountContext.Add(organisationOne);
        var organisationTwo = CreateOrganisation(nationId: Data.DbConstants.Nation.Scotland);
        _accountContext.Add(organisationTwo);
        _accountContext.Add(CreateOrganisation(nationId: Data.DbConstants.Nation.Scotland));

        var complianceScheme = CreateComplianceScheme(organisationOne.CompaniesHouseNumber, Data.DbConstants.Nation.England);
        var selectedSchemes = CreateSelectedSchemes(organisationConnectionId: 1, complianceSchemeId: 1, externalId: Guid.NewGuid());

        var organisationsConnections = CreateOrganisationsConnections(fromOrganisationId: 2, fromOrganisationRoleId: 2, toOrganisationId: 3, toOrganisationRoleId: 1, externalId: Guid.NewGuid());

        organisationOne.IsComplianceScheme = true;

        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.AddAsync(organisationsConnections, default);
        await _accountContext.AddAsync(selectedSchemes, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisationTwo.Name;

        // Act
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        // Assert
        results.Items.Count.Should().Be(1);

        AddAssert(results.Items[0], organisationTwo);
    }

    private static List<Data.Entities.Nation> CreateNations()
    {
        return new List<Data.Entities.Nation>
        {
            new() { Id = 0, Name = "Not Set", NationCode = "" },
            new() { Id = 1, Name = "England", NationCode = "GB-ENG" },
            new() { Id = 2, Name = "Northern Ireland", NationCode = "GB-NIR" },
            new() { Id = 3, Name = "Scotland", NationCode = "GB-SCT" },
            new() { Id = 4, Name = "Wales", NationCode = "GB-WLS" }
        };
    }

    private static SelectedScheme CreateSelectedSchemes(int organisationConnectionId, int complianceSchemeId, Guid externalId)
    {
        return new SelectedScheme
        {
            OrganisationConnectionId = organisationConnectionId,
            ComplianceSchemeId = complianceSchemeId,
            ExternalId = externalId
        };
    }

    private static OrganisationsConnection CreateOrganisationsConnections(int fromOrganisationId, int fromOrganisationRoleId, int toOrganisationId, int toOrganisationRoleId, Guid externalId)
    {
        return new OrganisationsConnection
        {
            FromOrganisationId = fromOrganisationId,
            FromOrganisationRoleId = fromOrganisationRoleId,
            ToOrganisationId = toOrganisationId,
            ToOrganisationRoleId = toOrganisationRoleId,
            ExternalId = externalId
        };
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWithRegulatorOrgReferenceNumber_ThenReturnNone()
    {
        var organisation = CreateOrganisation();
        organisation.OrganisationTypeId = Data.DbConstants.OrganisationType.Regulators;
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.ReferenceNumber;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        results.Items.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWithRegulatorOrgName_ThenReturnNone()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        organisation.OrganisationTypeId = Data.DbConstants.OrganisationType.Regulators;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        results.Items.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermForOrgWhichHasNullReferenceNumber_ThenReturnsCorrectData()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        organisation.Name = "org with null reference number";
        organisation.ReferenceNumber = null;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichIsCaseInsensitive_ThenReturnsCorrectData()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        organisation.Name = "Case Insensitive Test Organisation";

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);

        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = "cASE iNSENSITIVE tEST oRGANISATION";
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichHasDeletedComplianceScheme_ThenReturnsNoData()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland, true);

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;

        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);

        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichHasMismatchedNationIdButMatchingNationIdInComplianceScheme_ThenReturnsCorrectData()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        organisation.NationId = Data.DbConstants.Nation.NorthernIreland;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.Scotland, 3, 1);
        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichIsDeletedOrg_HaveMatchingComplianceScheme_ThenReturnsNoData()
    {
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);

        organisation.IsDeleted = true;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.Scotland, 3, 1);
        results.Items.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichIsComplianceSchemeMember_ThenReturnsIndirectOrgType()
    {
        var orgName = "Test Org";
        var organisation = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.Scotland);
        var organisationsConnections = CreateOrganisationsConnections(fromOrganisationId: 1, fromOrganisationRoleId: 2, toOrganisationId: 1, toOrganisationRoleId: 1, externalId: Guid.NewGuid());

        organisation.Name = orgName;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.AddAsync(organisationsConnections, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(orgName, Data.DbConstants.Nation.England, 3, 1);

        results.Items.FirstOrDefault().OrganisationType.Should().Be(OrganisationSchemeType.InDirectProducer.ToString());
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermWhichIsSubsidiaryOfAComplianceSchemeMember_ThenReturnsIndirectOrgType()
    {
        var orgName = "Test Org";
        var organisation = CreateOrganisation();
        var org2 = CreateOrganisation();
        var complianceScheme = CreateComplianceScheme(organisation.CompaniesHouseNumber, Data.DbConstants.Nation.England);
        var organisationsConnections = CreateOrganisationsConnections(fromOrganisationId: 1, fromOrganisationRoleId: 2, toOrganisationId: 1, toOrganisationRoleId: 1, externalId: Guid.NewGuid());
        var organisationRelationShip = new OrganisationRelationship { FirstOrganisationId = 1, SecondOrganisationId = 2 };

        org2.Name = orgName;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(org2, default);
        await _accountContext.AddAsync(complianceScheme, default);
        await _accountContext.AddAsync(organisationsConnections, default);
        await _accountContext.AddAsync(organisationRelationShip, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(orgName, Data.DbConstants.Nation.England, 3, 1);

        results.Items.FirstOrDefault().OrganisationType.Should().Be(OrganisationSchemeType.InDirectProducer.ToString());
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermIsNotComplianceSchemeMember_ThenReturnsDirectOrgType()
    {
        var orgName = "Test Org";
        var organisation = CreateOrganisation();

        await _accountContext.AddAsync(organisation, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(orgName, Data.DbConstants.Nation.England, 3, 1);

        results.Items.FirstOrDefault().OrganisationType.Should().Be(OrganisationSchemeType.DirectProducer.ToString());
    }

    [TestMethod]
    public async Task WhenGetOrganisationsBySearchTermIsSubsidiaryOfDirectProducer_ThenReturnsDirectOrgType()
    {
        var orgName = "Test Org";
        var organisation = CreateOrganisation();
        var organisationRelationShip = new OrganisationRelationship { FirstOrganisationId = 1, SecondOrganisationId = 1 };


        await _accountContext.AddAsync(organisation, default);
        await _accountContext.AddAsync(organisationRelationShip, default);

        _ = await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var results = await _organisationService.GetOrganisationsBySearchTerm(orgName, Data.DbConstants.Nation.England, 3, 1);

        results.Items.FirstOrDefault().OrganisationType.Should().Be(OrganisationSchemeType.DirectProducer.ToString());
    }

    [TestMethod]
    public async Task WhenGetOrganisationsByOrgNameSearchTermWhichHasNoComplianceScheme_HaveMatchingOrgNationId_ThenReturnsCorrectData()
    {
        var organisation = CreateOrganisation();
        organisation.NationId = Data.DbConstants.Nation.Wales;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.Name;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.Wales, 3, 1);
        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    [TestMethod]
    public async Task WhenGetOrganisationsByReferenceNumberSearchTermWhichHasNoComplianceScheme_HaveMatchingOrgNationId_ThenReturnsCorrectData()
    {
        var organisation = CreateOrganisation();
        organisation.NationId = Data.DbConstants.Nation.Wales;

        await _accountContext.AddAsync(organisation, default);
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty, default);

        var query = organisation.ReferenceNumber;
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.Wales, 3, 1);
        results.Items.Count.Should().Be(1);
        AddAssert(results.Items[0], organisation);
    }

    private static Organisation CreateOrganisation(int? nationId = Data.DbConstants.Nation.England)
    {
        return new Organisation
        {
            CompaniesHouseNumber = $"CompaniesHouseNumber_{Guid.NewGuid()}",
            ReferenceNumber = $"ReferenceNumber_{Guid.NewGuid()}",
            ExternalId = Guid.NewGuid(),
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            Name = $"Test org {Guid.NewGuid()}",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            SubBuildingName = "Sub building 8",
            BuildingName = "Building 8",
            BuildingNumber = "8",
            Street = "Street 8",
            Locality = "Locality 8",
            DependentLocality = "Dependent Locality 8",
            Town = "Town 8",
            County = "County 8",
            Postcode = "BT44 5QW",
            Country = "Country 8",
            NationId = nationId,
            IsDeleted = false,
        };
    }

    private static ComplianceScheme CreateComplianceScheme(string companiesHouseNumber, int? nationId, bool isDeleted = false)
    {
        return new ComplianceScheme
        {
            CompaniesHouseNumber = companiesHouseNumber,
            NationId = nationId,
            Name = $"Some name {Guid.NewGuid()}",
            IsDeleted = isDeleted,
            ExternalId = Guid.NewGuid()
        };
    }

    private static void AddAssert(OrganisationSearchResult actual, Organisation expected)
    {
        actual.OrganisationId.Should().Be(expected.ReferenceNumber);
        actual.CompanyHouseNumber.Should().Be(expected.CompaniesHouseNumber);
        actual.ExternalId.Should().Be(expected.ExternalId);
        actual.IsComplianceScheme.Should().Be(expected.IsComplianceScheme);
        actual.OrganisationName.Should().Be(expected.Name);
    }
}
