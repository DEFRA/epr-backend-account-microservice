using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackendAccountService.ValidationData.Api.UnitTests.Services;

[TestClass]
public class OrganisationDataServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private OrganisationDataService _organisationService = null!;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("OrganisationDataServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationDataService(_accountContext);
    }


    [TestMethod]
    public async Task GetOrganisationByExternalId_NoOrganisationExists_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = Guid.Empty;

        // Act
        var result = await _organisationService.GetOrganisationByExternalId(expectedOrganisation);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetOrganisationByExternalId_IsNotComplianceScheme_ThenReturnOrgWithEmptyMembers()
    {
        // Arrange
        var expectedOrganisation = _accountContext.Organisations.First();

        // Act
        var result = await _organisationService.GetOrganisationByExternalId(expectedOrganisation.ExternalId);

        // Assert
        result.Should().BeOfType(typeof(OrganisationResponse));
        result.IsComplianceScheme.Should().Be(expectedOrganisation.IsComplianceScheme);
        result.ReferenceNumber.Should().Be(expectedOrganisation.ReferenceNumber);
    }

    [TestMethod]
    public async Task GetOrganisationWithMembers_HasMembers_ThenReturnOrgWithMembers()
    {
        // Arrange
        var complianceSchemeId = new Guid("22222222-0000-0000-0000-000000000000");
        var expectedComplianceSchemeOrganisation = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "CS1234");
        var expectedMember = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "3000000");

        // Act
        var result = await _organisationService.GetOrganisationMembersByComplianceSchemeId(
            expectedComplianceSchemeOrganisation.ExternalId,
            complianceSchemeId);

        // Assert
        result.Should().BeOfType(typeof(OrganisationMembersResponse));
        result.MemberOrganisations.Should().NotBeEmpty();
        result.MemberOrganisations.First().Should().Be(expectedMember.ReferenceNumber);
    }

    [TestMethod]
    public async Task GetOrganisationWithMembers_HasNoMembers_ThenReturnEmpty()
    {
        // Arrange
        var complianceSchemeId = new Guid("22222222-0000-0000-0000-000000000001");
        var expectedComplianceSchemeOrganisation = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "CS1234");

        // Act
        var result = await _organisationService.GetOrganisationMembersByComplianceSchemeId(
            expectedComplianceSchemeOrganisation.ExternalId,
            complianceSchemeId);

        // Assert
        result.Should().BeOfType(typeof(OrganisationMembersResponse));
        result.MemberOrganisations.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetOrganisationWithMembers_ComplianceSchemeNull_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = _accountContext.Organisations.First().ExternalId;
        Guid? expectedComplianceScheme = null;

        // Act
        var result = await _organisationService.GetOrganisationMembersByComplianceSchemeId(
            expectedOrganisation,
            expectedComplianceScheme);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetOrganisationWithMembers_ComplianceSchemeDoesNotExist_ThenReturnNull()
    {
        // Arrange
        Guid? complianceSchemeId = new Guid("12345678-1234-1234-1234-123456789123");
        var expectedComplianceSchemeOrganisation = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "CS1234");

        // Act
        var result = await _organisationService.GetOrganisationMembersByComplianceSchemeId(
            expectedComplianceSchemeOrganisation.ExternalId,
            complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetExistingOrganisationsByReferenceNumber_OrgExists_ThenReturnReferenceNumbers()
    {
        // Arrange
        var expectedReferenceNumber = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "3000000").ReferenceNumber;

        // Act
        var result = await _organisationService.GetExistingOrganisationsByReferenceNumber(
            new List<string> { expectedReferenceNumber });

        // Assert
        result.Should().BeOfType(typeof(OrganisationsResponse));
        result.ReferenceNumbers.Should().NotBeEmpty();
        result.ReferenceNumbers.First().Should().Be(expectedReferenceNumber);
    }

    [TestMethod]
    public async Task GetExistingOrganisationsByReferenceNumber_OrgNotExist_ThenOrgNotInReturnedList()
    {
        // Arrange
        var expectedReferenceNumber = _accountContext.Organisations.FirstOrDefault(x => x.ReferenceNumber == "3000000").ReferenceNumber;
        const string invalidReferenceNumber = "ThisIsInvalidRef";

        // Act
        var result = await _organisationService.GetExistingOrganisationsByReferenceNumber(
            new List<string> { expectedReferenceNumber, invalidReferenceNumber });

        // Assert
        result.Should().BeOfType(typeof(OrganisationsResponse));
        result.ReferenceNumbers.Should().NotBeEmpty();
        result.ReferenceNumbers.Count().Should().Be(1);
        result.ReferenceNumbers.First().Should().Be(expectedReferenceNumber);
    }


    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var organisation1 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "123456",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 1",
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
            NationId = Data.DbConstants.Nation.NorthernIreland,
            ExternalId = new Guid("10000000-0000-0000-0000-000000000001")
        };
        setupContext.Organisations.Add(organisation1);

        var complianceSchemeOrganisation1 = new Organisation
        {
            Name = "Compliance scheme 1",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123456",
            ReferenceNumber = "CS1234"
        };
        var complianceSchemeOrganisation2 = new Organisation
        {
            Name = "Compliance scheme 2",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000002"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123457",
            ReferenceNumber = "CS1235"
        };

        var complianceSchemeOrganisation3 = new Organisation
        {
            Name = "Compliance scheme 3",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000003"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123458",
            ReferenceNumber = "CS1236"
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


        var member2 = new Organisation
        {
            Name = $"Test",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = Guid.NewGuid(),
            IsComplianceScheme = false,
            ReferenceNumber = "3000000"
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
            ComplianceScheme = complianceScheme1,
            ExternalId = new Guid("44444444-0000-0000-0000-000000000001")
        };
        setupContext.SelectedSchemes.Add(selectedScheme2);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}