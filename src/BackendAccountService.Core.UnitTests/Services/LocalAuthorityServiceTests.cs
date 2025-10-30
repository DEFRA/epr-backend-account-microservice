using System.Net;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Nation = BackendAccountService.Core.Models.Nation;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class LocalAuthorityServiceTests
{
    private AccountsDbContext _dbContext;
    private LocalAuthorityService _laService = null!;
    private readonly int laOrgType = Data.DbConstants.OrganisationType.WasteDisposalAuthority;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("LocalAuthorityServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _dbContext = new AccountsDbContext(contextOptions);
        var logger = new Mock<ILogger<ComplianceSchemeService>>();
        _laService = new LocalAuthorityService(_dbContext, logger.Object);
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var organisation1 = new Organisation
        {
            Id = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000001"),
            OrganisationTypeId = Data.DbConstants.OrganisationType.WasteDisposalAuthority,
            CompaniesHouseNumber = "12345678",
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
            ReferenceNumber = "12345678",
            TradingName = "Trading 1",
            IsDeleted = false
        };

        var organisation2 = new Organisation
        {
            Id = 2,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000002"),
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "12345678",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 2",
            SubBuildingName = "Sub building 2",
            BuildingName = "Building 2",
            BuildingNumber = "2",
            Street = "Street 2",
            Locality = "Locality 2",
            DependentLocality = "Dependent Locality 2",
            Town = "Town 2",
            County = "County 2",
            NationId = Data.DbConstants.Nation.England,
            Postcode = "BT44 5QW",
            Country = "Country 2",
            ReferenceNumber = "12345678",
            TradingName = "Trading 2",
            IsDeleted = false
        };
        setupContext.Organisations.Add(organisation1);
        setupContext.Organisations.Add(organisation2);

        var localAuthOrganisation1 = new LaOrganisation
        {
            Id = 1,
            DistrictCode = "ABCDEF",
            Organisation = organisation1,
            OrganisationId = 1,
            IsDeleted = false
        };

        var localAuthOrganisation2 = new LaOrganisation
        {
            Id = 2,
            DistrictCode = "ABCDEF2",
            Organisation = organisation2,
            OrganisationId = 2,
            IsDeleted = false
        };
        setupContext.LaOrganisations.Add(localAuthOrganisation1);
        setupContext.LaOrganisations.Add(localAuthOrganisation2);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    [TestMethod]
    public async Task WhenValidRequest_ThenLocalAuthorityIsCreated()
    {
        //Arrange
        var request = new CreateLocalAuthorityRequest
        {
            WasteAuthorityType = "Waste Disposal Authority",
            CompaniesHouseNumber = "123456",
            Name = "MockName",
            TradingName = "MockTradingName",
            ReferenceNumber = "12345678",
            BuildingName = "building",
            BuildingNumber = "1",
            Street = "street",
            Town = "Town",
            County = "County",
            Country = "Country",
            Postcode = "BT7 1NT",
            Nation = "England",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = false,
            DistrictCode = "123456"
        };

        //Act
        var result = await _laService.CreateNewLocalAuthorityOrganisationAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        result.Value.Nation.Should().Be(Nation.England);
        result.Value.OrganisationType.Should().Be(OrganisationType.WasteDisposalAuthority);
    }

    [TestMethod]
    public async Task WhenRequestNotValid_ThenLocalAuthorityNotCreated_ReturnFailedResult()
    {
        //Arrange
        var request = It.IsAny<CreateLocalAuthorityRequest>();

        //Act
        var result = await _laService.CreateNewLocalAuthorityOrganisationAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.ErrorMessage.Should().Be("Error creating new Local Authority / Organisation");
    }

    [TestMethod]
    public async Task GetAllLocalAuthorities_ThenReturnList()
    {
        //Setup
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationAsync();

        //Assert

        result.Should().BeOfType(typeof(List<LocalAuthorityResponseModel>));
        result.Count.Should().Be(2);
        result.First().Name.Should().Be(expectedLaOrganisation.Organisation.Name);
    }

    [TestMethod]
    public async Task GetLocalAuthorityOrganisationByOrganisationTypeId_TheReturnList()
    {
        //Setup
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(laOrgType);

        //Assert

        result.Should().BeOfType(typeof(List<LocalAuthorityResponseModel>));
        result.Count.Should().Be(1);
        result.First().Name.Should().Be(expectedLaOrganisation.Organisation.Name);
    }

    [TestMethod]
    public async Task GetLocalAuthorityOrganisationByExternalId_ThenReturnLaOrg()
    {
        //Setup
        var expectedOrganisation = _dbContext.Organisations.First();
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationByExternalIdAsync(expectedOrganisation.ExternalId.ToString());

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(expectedLaOrganisation.Organisation.Name);
    }

    [TestMethod]
    public async Task GetLocalAuthorityOrganisationByExternalId_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var mockOrgId = It.IsAny<Guid>().ToString();

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationByExternalIdAsync(mockOrgId);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetLocalAuthorityByDistrictCode_ThenReturnLaOrg()
    {
        //Setup
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();

        //Act
        var result = await _laService.GetLocalAuthorityByDistrictCodeAsync(expectedLaOrganisation.DistrictCode);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(expectedLaOrganisation.Organisation.Name);
    }

    [TestMethod]
    public async Task GetLocalAuthorityByDistrictCode_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var mockDistrictCode = It.IsAny<string>();

        //Act
        var result = await _laService.GetLocalAuthorityByDistrictCodeAsync(mockDistrictCode);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetLocalAuthorityByOrganisationName_ThenReturnLaOrg()
    {
        //Setup
        var expectedOrganisation = _dbContext.Organisations.First();
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();

        //Act
        var result = await _laService.GetLocalAuthorityByOrganisationNameAsync(expectedOrganisation.Name);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(expectedLaOrganisation.Organisation.Name);
    }

    [TestMethod]
    public async Task GetLocalAuthorityByOrganisationName_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var mockOrgName = It.IsAny<string>();

        //Act
        var result = await _laService.GetLocalAuthorityByOrganisationNameAsync(mockOrgName);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdateLocalAuthorityByDistrictCode_ThenReturnSuccess()
    {
        //Setup
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();
        UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
        {
            UserId = It.IsAny<Guid>(),
            DistrictCode = expectedLaOrganisation.DistrictCode,
            Name = "New Name",
            Nation = "Northern Ireland",
            WasteAuthorityType = "Waste Collection Authority"
        };


        //Act
        var result = await _laService.UpdateLocalAuthorityByDistrictCodeAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        result.Value.DistrictCode.Should().Be(expectedLaOrganisation.DistrictCode);
        result.Value.Nation.Value.Should().Be(Nation.NorthernIreland);
        result.Value.OrganisationType.Should().Be(OrganisationType.WasteCollectionAuthority);
    }

    [TestMethod]
    public async Task UpdateLocalAuthorityByDistrictCode_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var request = new UpdateLocalAuthorityRequest
        {
            DistrictCode = It.IsAny<string>()
        };

        //Act
        var result = await _laService.UpdateLocalAuthorityByDistrictCodeAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdateLocalAuthorityByExternalId_ThenReturnSuccess()
    {
        //Setup
        var expectedOrganisation = _dbContext.Organisations.First();
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();
        UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
        {
            UserId = It.IsAny<Guid>(),
            ExternalId = expectedOrganisation.ExternalId.ToString(),
            DistrictCode = expectedLaOrganisation.DistrictCode,
            Name = "New Name",
            Nation = "Scotland",
            WasteAuthorityType = "Waste Collection Authority"
        };


        //Act
        var result = await _laService.UpdateLocalAuthorityByExternalIdAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        result.Value.DistrictCode.Should().Be(expectedLaOrganisation.DistrictCode);
        result.Value.Nation.Value.Should().Be(Nation.Scotland);
        result.Value.OrganisationType.Should().Be(OrganisationType.WasteCollectionAuthority);
    }

    [TestMethod]
    public async Task UpdateLocalAuthorityByExternalId_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var request = new UpdateLocalAuthorityRequest
        {
            ExternalId = It.IsAny<Guid>().ToString()
        };

        //Act
        var result = await _laService.UpdateLocalAuthorityByExternalIdAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result<LocalAuthorityResponseModel>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task RemoveLocalAuthorityByDistrictCode_ThenReturnSuccess()
    {
        //Setup
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();
        RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest
        {
            UserId = It.IsAny<Guid>(),
            DistrictCode = expectedLaOrganisation.DistrictCode
        };


        //Act
        var result = await _laService.RemoveLocalAuthorityByDistrictCodeAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();
    }
    [TestMethod]
    public async Task RemoveLocalAuthorityByDistrictCode_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var request = new RemoveLocalAuthorityRequest()
        {
            DistrictCode = It.IsAny<string>()
        };

        //Act
        var result = await _laService.RemoveLocalAuthorityByDistrictCodeAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [TestMethod]
    public async Task RemoveLocalAuthorityByExternalId_ThenReturnSuccess()
    {
        //Setup
        var expectedOrganisation = _dbContext.Organisations.First();
        var expectedLaOrganisation = _dbContext.LaOrganisations.First();
        RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest
        {
            UserId = It.IsAny<Guid>(),
            DistrictCode = expectedLaOrganisation.DistrictCode,
            ExternalId = expectedOrganisation.ExternalId.ToString()
        };


        //Act
        var result = await _laService.RemoveLocalAuthorityByExternalIdAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveLocalAuthorityByExternalId_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var request = new RemoveLocalAuthorityRequest()
        {
            ExternalId = It.IsAny<Guid>().ToString()
        };

        //Act
        var result = await _laService.RemoveLocalAuthorityByExternalIdAsync(request);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [TestMethod]
    public async Task GetLocalAuthorityOrganisationNation_ThenReturnSuccess()
    {
        //Setup
        var expectedNation = "Northern Ireland";

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationNationAsync(expectedNation);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetLocalAuthorityByNation_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var mockOrgNation = "MockNation";

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationNationAsync(mockOrgNation);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetLocalAuthorityOrganisationByOrganisationTypeName_ThenReturnSuccess()
    {
        //Setup
        var expectedType = "Waste Disposal Authority";

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(expectedType);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetLocalAuthorityByTypeName_DoesNotExist_ThenReturnFailed()
    {
        //Arrange
        var mockType = "MockType";

        //Act
        var result = await _laService.GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(mockType);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}