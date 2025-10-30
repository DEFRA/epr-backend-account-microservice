using AutoFixture.AutoMoq;
using AutoFixture;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class BulkUploadControllerTests
{
    private BulkUploadController _bulkUploadController = null!;
    private Mock<IOrganisationService> _organisationServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<BulkUploadController> _nullLogger = new();
    private Mock<IValidationService> _validateDataService = null!;
    private readonly Guid _orgExternalId = Guid.NewGuid();
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });
        _validateDataService = new Mock<IValidationService>();
        _organisationServiceMock = new Mock<IOrganisationService>();
        _userServiceMock = new Mock<IUserService>();
        _bulkUploadController = new BulkUploadController(_organisationServiceMock.Object,
            _apiConfigOptionsMock.Object, _nullLogger, _validateDataService.Object, _userServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }


    [TestMethod]
    public async Task AddSubsidiaryRelationship_Returns_400BadRequest_When_Invalid_UserId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 2129352 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync((User)null);

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(new BulkSubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
    }

    [TestMethod]
    public async Task CreateSubsidiaryAndRelationship_Returns_Ok_With_Organisation()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 2129352 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352", OrganisationRelationships = new List<OrganisationRelationship>() });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.CreateSubsidiaryAndRelationship(new BulkOrganisationRequestModel()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("2129352");
    }

    [TestMethod]
    public async Task CreateSubsidiaryAndRelationship_Returns_null_when_invalid_Organisation()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 2129352 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352", OrganisationRelationships = null });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.CreateSubsidiaryAndRelationship(new BulkOrganisationRequestModel()) as ObjectResult;

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task CreateSubsidiaryAndRelationship_Returns_Ok_With_franchiseeOrganisation()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 2129352 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352", OrganisationRelationships = new List<OrganisationRelationship>() });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.CreateSubsidiaryAndRelationship(new BulkOrganisationRequestModel() { Subsidiary = new OrganisationModel() { Franchisee_Licensee_Tenant = "Y" } } ) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("2129352");
    }


    [TestMethod]
    public async Task CreateSubsidiaryAndRelationship_Returns_400BadRequest_When_Invalid_OrganisationId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.CreateSubsidiaryAndRelationship(new BulkOrganisationRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("organisation does not exist");
        resultValue.Type.Should().Be("organisation/invalid-externalId");
    }

    [TestMethod]
    public async Task CreateSubsidiaryAndRelationship_Returns_400BadRequest_When_Invalid_UserId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 2129352 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "2129352" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync((User)null);

        // Act
        var result = await _bulkUploadController.CreateSubsidiaryAndRelationship(new BulkOrganisationRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
    }

    [TestMethod]
    public async Task AddSubsidiaryRelationship_Returns_Ok_With_Organisation()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456" });

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456" });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(new BulkSubsidiaryAddRequestModel { ChildOrganisationId = "123456", JoinerDate = DateTime.Now.ToString() }) as ObjectResult;

        // Assert
        result.Should().NotBeNull();

        result.Value.Should().Be("Ref:123456");
    }

    [TestMethod]
    public async Task SubsidiaryRelationship_Returns_Ok_With_Org()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456" });

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456", JoinerDate = "10/10/2024" });


        _organisationServiceMock
            .Setup(service => service.UpdateOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.UpdateSubsidiaryRelationship(new BulkSubsidiaryUpdateRequestModel()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("Ref:123456");
    }

    [TestMethod]
    public async Task AddSubsidiaryRelationship_Returns_Ok_With_Organisation_With_Franchisee()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456", Franchisee_Licensee_Tenant = "Y" });

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456", Franchisee_Licensee_Tenant = "Y" });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(new BulkSubsidiaryAddRequestModel { ChildOrganisationId = "123456" }) as ObjectResult;

        // Assert
        result.Should().NotBeNull();

        result.Value.Should().Be("Ref:123456");
    }


    [TestMethod]
    public async Task AddSubsidiaryRelationship_Returns_NotFound()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456"});

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(null) as ObjectResult;

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task AddSubsidiaryRelationship_Returns_400_WhenInvalidChildOrganisationId_With_Org()
    {
        // Arrange
        _organisationServiceMock
            .SetupSequence(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(
            new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:45145342" }).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(new BulkSubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("child organisation does not exist");
        resultValue.Type.Should().Be("child organisation/invalid-childOrganisationId");
    }

    [TestMethod]
    public async Task AddSubsidiaryRelationship__Returns_400BadRequest_When_Invalid_OrganisationId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "0123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.AddSubsidiaryRelationship(new BulkSubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("organisation does not exist");
        resultValue.Type.Should().Be("organisation/invalid-externalId");
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseNumber_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel> { new OrganisationResponseModel { Name = "Test Org" } });

        // Act
        var result = await _bulkUploadController.GetOrganisationsByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<OrganisationResponseModel>>();
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseName_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel> { new OrganisationResponseModel { Name = "Test Org" } });

        // Act
        var result = await _bulkUploadController.GetOrganisationsByCompaniesHouseName(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<OrganisationResponseModel>>();
    }

    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel> { new OrganisationResponseModel { Name = "1234567" } });

        // Act
        var result = await _bulkUploadController.GetOrganisationByReferenceNumberAsync(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<OrganisationResponseModel>>();
    }

    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_Returnsnull()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByReferenceNumber(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel> {});

        // Act
        var result = await _bulkUploadController.GetOrganisationByReferenceNumberAsync(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseName_whenInvalidCompanyName_ReturnsNoContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _bulkUploadController.GetOrganisationsByCompaniesHouseName(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().BeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseNumber_whenInvalidCompanyHouseNumber_ReturnsNoContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _bulkUploadController.GetOrganisationsByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().BeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task OrganisationExternalId_WhenUserValid_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByExternalId(_orgExternalId))
            .ReturnsAsync(new OrganisationDetailModel());

        // Act
        var result = await _bulkUploadController.GetOrganisationByExternalIdAsync(_orgExternalId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<OrganisationDetailModel>();
    }

    [TestMethod]
    public async Task OrganisationExternalId_WhenUserNotValid_ReturnsNoContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByExternalId(_orgExternalId))
            .ReturnsAsync((OrganisationDetailModel?)null);

        // Act
        var result = await _bulkUploadController.GetOrganisationByExternalIdAsync(_orgExternalId) as ObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetOrganisationsRelationshipsByCompaniesHouseNumber_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.IsOrganisationInRelationship(129360, 129231))
            .ReturnsAsync((false));

        // Act
        var result = await _bulkUploadController.GetOrganisationRelationshipAsync(129360, 129231) as ObjectResult;

        // Assert
        result.Should().BeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        result?.Value.Should().BeNull();
    }

    [TestMethod]
    public async Task TerminateSubsidiary_Returns_400BadRequest_When_Invalid_OrganisationId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "0123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.TerminateSubsidiaryRelationship(new BulkSubsidiaryTerminateRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("organisation does not exist");
        resultValue.Type.Should().Be("organisation/invalid-externalId");
    }

    [TestMethod]
    public async Task TerminateSubsidiary_Returns_400BadRequest_When_Invalid_UserId()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 123456 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "0123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync((User)null);

        // Act
        var result = await _bulkUploadController.TerminateSubsidiaryRelationship(new BulkSubsidiaryTerminateRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
    }

    [TestMethod]
    public async Task TerminateSubsidiary_Returns_Ok()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123" });

        _organisationServiceMock
            .Setup(service => service.TerminateOrganisationRelationshipsAsync(
                It.IsAny<TerminateSubsidiaryModel>()))
            .ReturnsAsync(Result.SuccessResult());

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _bulkUploadController.TerminateSubsidiaryRelationship(new BulkSubsidiaryTerminateRequestModel{ ChildOrganisationId = "1234" }) as OkResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}