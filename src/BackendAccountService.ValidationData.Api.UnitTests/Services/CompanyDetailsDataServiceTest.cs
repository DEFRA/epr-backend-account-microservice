using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BackendAccountService.ValidationData.Api.UnitTests.Services;

[TestClass]
public class CompanyDetailsDataServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private CompanyDetailsDataService _companyDetailsService = null!;
    private List<CompanyDetailResponse> _companyDetailsResponseList = null!;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("CompanyDetailsDataServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _companyDetailsService = new CompanyDetailsDataService(_accountContext);
        _companyDetailsResponseList = new List<CompanyDetailResponse>();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationReferenceNumber_NoOrganisationExists_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = string.Empty;

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumber(expectedOrganisation);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationReferenceNumber_HasMembers_ThenReturnOrgWithMembers()
    {
        // Arrange
        var expectedOrganisation = "123456";
        var expectedMember = await _accountContext.Organisations.AsNoTracking()
            .Where(organisation => organisation.ReferenceNumber == expectedOrganisation)
            .ToListAsync();
        foreach (var org in expectedMember)
        {
            _companyDetailsResponseList.Add(new CompanyDetailResponse { ReferenceNumber = org.ReferenceNumber, CompaniesHouseNumber = org.CompaniesHouseNumber });
        }
        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = _companyDetailsResponseList
        };

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumber(expectedOrganisation);

        // Assert
        result.Should().BeOfType(typeof(CompanyDetailsResponse));
        result.Organisations.Should().NotBeEmpty();
        result.Organisations.Equals(organisationsResult.Organisations);
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationExternalId_NoOrganisationExists_ThenReturnNull()
    {
        // Arrange
        var externalId = Guid.NewGuid();

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationExternalId(externalId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationExternalId_HasMembers_ThenReturnOrgWithMembers()
    {
        // Arrange
        var externalId = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var expectedMember = await _accountContext.Organisations.AsNoTracking()
            .Where(organisation => organisation.ExternalId == externalId)
            .ToListAsync();

        foreach (var org in expectedMember)
        {
            _companyDetailsResponseList.Add(new CompanyDetailResponse { ReferenceNumber = org.ReferenceNumber, CompaniesHouseNumber = org.CompaniesHouseNumber });
        }

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = _companyDetailsResponseList
        };

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationExternalId(externalId);

        // Assert
        result.Should().BeOfType(typeof(CompanyDetailsResponse));
        result.Organisations.Should().NotBeEmpty();
        result.Organisations.Equals(organisationsResult.Organisations);
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId_NoOrganisationExists_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = string.Empty;
        var expectedComplianceScheme = Guid.Empty;

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(expectedOrganisation, expectedComplianceScheme);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId_HasMembers_ThenReturnOrgWithMembers()
    {
        // Arrange
        var expectedOrganisation = "3000000";
        var complianceSchemeId = new Guid("22222222-0000-0000-0000-000000000000");

        var complianceScheme = _accountContext.ComplianceSchemes
           .AsNoTracking()
           .FirstOrDefault(complianceScheme => complianceScheme.ExternalId == complianceSchemeId);

        var selectedSchemes = await _accountContext.SelectedSchemes
            .AsNoTracking()
            .Where(selectedScheme =>
                selectedScheme.ComplianceSchemeId == complianceScheme.Id
            )
            .Select(member => new
            {
                ReferenceNumber = member.OrganisationConnection.FromOrganisation.ReferenceNumber.ToString(),
                CompaniesHouseNumber = member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber != null
                ? member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber.ToString()
                : null
            })
            .ToListAsync();

        foreach (var org in selectedSchemes)
        {
            _companyDetailsResponseList.Add(new CompanyDetailResponse { ReferenceNumber = org.ReferenceNumber, CompaniesHouseNumber = org.CompaniesHouseNumber });
        }
        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = _companyDetailsResponseList
        };

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(expectedOrganisation, complianceSchemeId);

        // Assert
        result.Should().BeOfType(typeof(CompanyDetailsResponse));
        result.Organisations.Should().NotBeEmpty();
        result.Organisations.Equals(organisationsResult.Organisations);
    }

    [TestMethod]
    public async Task GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId_DoesNotExist_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = "3000000";
        var complianceSchemeId = Guid.Empty;

        // Act
        var result = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(expectedOrganisation, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAllProducersCompanyDetails_NoOrganisationExists_ThenReturnNull()
    {
        // Arrange
        var expectedOrganisation = new List<string>() {};

        // Act
        var result = await _companyDetailsService.GetAllProducersCompanyDetails(expectedOrganisation);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAllProducersCompanyDetails_HasMembers_ThenReturnOrgWithMembers()
    {
        // Arrange
        var expectedOrganisation = new List<string>() { "123456", "123466" };

        var organisations = await _accountContext.Organisations
           .AsNoTracking()
           .Where(organisation => expectedOrganisation.Contains(organisation.ReferenceNumber) && !organisation.IsComplianceScheme && (organisation.OrganisationTypeId == 1 || organisation.OrganisationTypeId == 2))
           .ToListAsync();

        foreach (var org in organisations)
        {
            _companyDetailsResponseList.Add(new CompanyDetailResponse { ReferenceNumber = org.ReferenceNumber, CompaniesHouseNumber = org.CompaniesHouseNumber });
        }
        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = _companyDetailsResponseList
        };

        // Act
        var result = await _companyDetailsService.GetAllProducersCompanyDetails(expectedOrganisation);

        // Assert
        result.Should().BeOfType(typeof(CompanyDetailsResponse));
        result.Organisations.Should().NotBeEmpty();
        result.Organisations.Equals(organisationsResult.Organisations);
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
        var organisation2 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345688",
            ReferenceNumber = "123466",
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
        setupContext.Organisations.Add(organisation2);

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