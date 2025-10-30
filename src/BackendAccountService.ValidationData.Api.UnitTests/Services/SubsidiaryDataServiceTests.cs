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
public class SubsidiaryDataServiceTests
{
    private readonly Guid _organisation1Id = Guid.NewGuid();
    private AccountsDbContext _accountContext = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("SubsidiaryDataServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            await SetUpDatabase(contextOptions);
        
        _accountContext = new AccountsDbContext(contextOptions);
    }

    [TestMethod]
    public async Task GetSubsidiaryDetails_ReturnsSubsidiaryExistenceDetails()
    {
        // Arrange
        var request = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails =
            [
                new()
                {
                    OrganisationReference = "ref1001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref1002" },
                        new SubsidiaryDetail { ReferenceNumber = "ref1003" },
                    ]
                },
                new()
                {
                    OrganisationReference = "ref2001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref2002" },
                        new SubsidiaryDetail { ReferenceNumber = "ref2003" },
                    ]
                }
            ]
        };

        var sut = new SubsidiaryDataService(_accountContext);

        // Act
        var result = await sut.GetSubsidiaryDetails(request);

        // Assert
        Assert.IsNotNull(result);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref1001").SubsidiaryDetails
            .Where(detail => !detail.SubsidiaryBelongsToAnyOtherOrganisation && detail.SubsidiaryExists).Should().HaveCount(2);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref2001").SubsidiaryDetails
            .Where(detail => !detail.SubsidiaryBelongsToAnyOtherOrganisation && detail.SubsidiaryExists).Should().HaveCount(2);
    }


    [TestMethod]
    public async Task GetSubsidiaryDetails_ReturnsSubsidiaryExistenceDetailsForSubsidiariesThetDoNotBelongToAnyOrganisation()
    {
        // Arrange
        var request = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails =
            [
                new()
                {
                    OrganisationReference = "ref3001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref3002" },
                        new SubsidiaryDetail { ReferenceNumber = "ref3003" },
                        new SubsidiaryDetail { ReferenceNumber = "ref3004" },
                    ]
                }
            ]
        };

        var sut = new SubsidiaryDataService(_accountContext);

        // Act
        var result = await sut.GetSubsidiaryDetails(request);

        // Assert
        Assert.IsNotNull(result);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref3001").SubsidiaryDetails
            .Where(detail => new[] { "ref3004" }.Contains(detail.ReferenceNumber)
                && !detail.SubsidiaryBelongsToAnyOtherOrganisation
                && detail.SubsidiaryDoesNotBelongToAnyOrganisation
                && detail.SubsidiaryExists).Should().HaveCount(1);

    }

    [TestMethod]
    public async Task GetSubsidiaryDetails_ReturnsSubsidiaryWithCompaniesHouseNumber()
    {
        // Arrange
        var request = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails =
            [
                new()
                {
                    OrganisationReference = "ref3001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref3002" },
                        new SubsidiaryDetail { ReferenceNumber = "ref3003" },
                        new SubsidiaryDetail { ReferenceNumber = "ref3004" },
                    ]
                }
            ]
        };

        var sut = new SubsidiaryDataService(_accountContext);

        // Act
        var result = await sut.GetSubsidiaryDetails(request);

        // Assert
        Assert.IsNotNull(result);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref3001").SubsidiaryDetails
            .Where(detail => new[] { "ref3004" }.Contains(detail.ReferenceNumber)
                && !string.IsNullOrEmpty(detail.CompaniesHouseNumber)
                && detail.SubsidiaryExists).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task GetSubsidiaryDetails_ReturnsSubsidiaryWithNoCompaniesHouseNumber()
    {
        // Arrange
        var request = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails =
            [
                new()
                {
                    OrganisationReference = "ref3001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref3005" },
                    ]
                }
            ]
        };

        var sut = new SubsidiaryDataService(_accountContext);

        // Act
        var result = await sut.GetSubsidiaryDetails(request);

        // Assert
        Assert.IsNotNull(result);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref3001").SubsidiaryDetails
            .Where(detail => new[] { "ref3005" }.Contains(detail.ReferenceNumber)
                && string.IsNullOrEmpty(detail.CompaniesHouseNumber)
                && detail.SubsidiaryExists).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task GetSubsidiaryDetails_DoesNotReturnCompaniesHouseNumber_WhenSubsidiaryNotFound()
    {
        // Arrange
        var request = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails =
            [
                new()
                {
                    OrganisationReference = "ref3001",
                    SubsidiaryDetails =
                    [
                        new SubsidiaryDetail { ReferenceNumber = "ref3XX9" },
                    ]
                }
            ]
        };

        var sut = new SubsidiaryDataService(_accountContext);

        // Act
        var result = await sut.GetSubsidiaryDetails(request);

        // Assert
        Assert.IsNotNull(result);
        result.SubsidiaryOrganisationDetails.Single(org => org.OrganisationReference == "ref3001").SubsidiaryDetails
            .Where(detail => new[] { "ref3XX9" }.Contains(detail.ReferenceNumber)
                && string.IsNullOrEmpty(detail.CompaniesHouseNumber)
                && !detail.SubsidiaryExists).Should().HaveCount(1);
    }

    private async Task SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        await using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        await setupContext.Database.EnsureDeletedAsync();
        await setupContext.Database.EnsureCreatedAsync();

        var organisation1 = new Organisation
        {
            Id = 1001,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref1001",
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
            ExternalId = _organisation1Id
        };

        var organisation2 = new Organisation
        {
            Id = 1002,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref1002",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 2",
            ExternalId = Guid.NewGuid()
        };

        var organisation3 = new Organisation
        {
            Id = 1003,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref1003",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 3",
            ExternalId = Guid.NewGuid()
        };

        var organisation4 = new Organisation
        {
            Id = 2001,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref2001",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 4",
            SubBuildingName = "Sub building 4",
            BuildingName = "Building 4",
            BuildingNumber = "4",
            Street = "Street 4",
            Locality = "Locality 4",
            DependentLocality = "Dependent Locality 4",
            Town = "Town 4",
            County = "County 4",
            Postcode = "BT44 5QW",
            Country = "Country 4",
            NationId = Data.DbConstants.Nation.NorthernIreland,
            ExternalId = _organisation1Id
        };

        var organisation5 = new Organisation
        {
            Id = 2002,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref2002",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 5",
            ExternalId = Guid.NewGuid()
        };

        var organisation6 = new Organisation
        {
            Id = 2003,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref2003",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 6",
            ExternalId = Guid.NewGuid()
        };

        var organisation7 = new Organisation
        {
            Id = 3001,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref3001",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 7",
            SubBuildingName = "Sub building 7",
            BuildingName = "Building 7",
            BuildingNumber = "7",
            Street = "Street 7",
            Locality = "Locality 7",
            DependentLocality = "Dependent Locality 7",
            Town = "Town 7",
            County = "County 7",
            Postcode = "BT44 5QW",
            Country = "Country 7",
            NationId = Data.DbConstants.Nation.NorthernIreland,
            ExternalId = _organisation1Id
        };

        var organisation8 = new Organisation
        {
            Id = 3002,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref3002",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 8",
            ExternalId = Guid.NewGuid()
        };

        var organisation9 = new Organisation
        {
            Id = 3003,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref3003",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 9",
            ExternalId = Guid.NewGuid()
        };

        var organisation10 = new Organisation
        {
            Id = 3004,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "12345678",
            ReferenceNumber = "ref3004",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 10",
            ExternalId = Guid.NewGuid()
        };

        var organisation11 = new Organisation
        {
            Id = 3005,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = string.Empty,
            ReferenceNumber = "ref3005",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 11",
            ExternalId = Guid.NewGuid()
        };

        var organisation12 = new Organisation
        {
            Id = 3006,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = string.Empty,
            ReferenceNumber = "ref3006",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 12",
            ExternalId = Guid.NewGuid(),
        };
        var organisation13 = new Organisation
        {
            Id = 3007,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = string.Empty,
            ReferenceNumber = "ref3007",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 13",
            ExternalId = Guid.NewGuid(),
        };
        await setupContext.Organisations.AddRangeAsync(organisation1, organisation2, organisation3, organisation4, organisation5, organisation6, organisation7, organisation8, organisation9, organisation10, organisation11, organisation12, organisation13);

        var relationship1 = new OrganisationRelationship
        {
            FirstOrganisationId = 1001,
            SecondOrganisationId = 1002,
            OrganisationRelationshipTypeId = 10007
        };

        var relationship2 = new OrganisationRelationship
        {
            FirstOrganisationId = 1001,
            SecondOrganisationId = 1003,
            OrganisationRelationshipTypeId = 10007
        };

        var relationship3 = new OrganisationRelationship
        {
            FirstOrganisationId = 2001,
            SecondOrganisationId = 2002,
            OrganisationRelationshipTypeId = 10007
        };

        var relationship4 = new OrganisationRelationship
        {
            FirstOrganisationId = 2001,
            SecondOrganisationId = 2003,
            OrganisationRelationshipTypeId = 10007
        };

        var relationship5 = new OrganisationRelationship
        {
            FirstOrganisationId = 3001,
            SecondOrganisationId = 3002,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = DateTime.Now
        };

        var relationship6 = new OrganisationRelationship
        {
            FirstOrganisationId = 3001,
            SecondOrganisationId = 3003,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = DateTime.Now.AddDays(1)
        };

        var relationship7 = new OrganisationRelationship
        {
            FirstOrganisationId = 2001,
            SecondOrganisationId = 3004,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = DateTime.Now
        };

        var relationship8 = new OrganisationRelationship
        {
            FirstOrganisationId = 3001,
            SecondOrganisationId = 3005,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = DateTime.Now
        };

        var relationship9 = new OrganisationRelationship
        {
            FirstOrganisationId = 3006,
            SecondOrganisationId = 3007,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = DateTime.Now.AddDays(1),
            JoinerDate = new DateTime(2024, 02, 20,0,0,0, DateTimeKind.Utc)
        };

        await setupContext.OrganisationRelationships.AddRangeAsync(relationship1, relationship2, relationship3, relationship4, relationship5, relationship6, relationship7, relationship8, relationship9);
        await setupContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
    }
}