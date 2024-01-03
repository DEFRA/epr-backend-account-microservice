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
    private AccountsDbContext _accountContext= null!;
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
        //Setup
        var expectedOrganisation = Guid.Empty;

        //Act
        var result = await _organisationService.GetOrganisationByExternalId(expectedOrganisation);

        //Assert

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetOrganisationByExternalId_IsNotComplianceScheme_ThenReturnOrgWithEmptyMembers()
    {
        //Setup
        var expectedOrganisation = _accountContext.Organisations.First();

        //Act
        var result = await _organisationService.GetOrganisationByExternalId(expectedOrganisation.ExternalId);

        //Assert

        result.Should().BeOfType(typeof(OrganisationResponse));
        result.IsComplianceScheme.Should().Be(expectedOrganisation.IsComplianceScheme);
        result.ReferenceNumber.Should().Be(expectedOrganisation.ReferenceNumber);
    }

    private void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
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
            ComplianceScheme  = complianceScheme1,
            ExternalId = new Guid("44444444-0000-0000-0000-000000000001")
        };
        setupContext.SelectedSchemes.Add(selectedScheme2);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}