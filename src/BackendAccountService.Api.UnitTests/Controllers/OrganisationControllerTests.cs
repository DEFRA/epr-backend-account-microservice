using System.Globalization;
using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class OrganisationControllerTests
{
    private OrganisationsController _organisationController = null!;
    private Mock<IOrganisationService> _organisationServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<IRegulatorService> _regulatorServiceMock = null!;
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<OrganisationsController> _nullLogger = new();
    private Mock<IValidationService> _validateDataService = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _orgExternalId = Guid.NewGuid();
    private const int serviceRoleId = 3;

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
        _regulatorServiceMock = new Mock<IRegulatorService>();

        _organisationController = new OrganisationsController(
            _organisationServiceMock.Object,
            _apiConfigOptionsMock.Object,
            _nullLogger,
            _validateDataService.Object,
            _userServiceMock.Object,
            _regulatorServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task When_User_List_For_Organisation_Is_Requested_With_Valid_Data_And_User_Is_Authorised_Then_Return_User_List_As_Response()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();

        _organisationServiceMock.Setup(service => service.GetUserListForOrganisation(_userId, _orgId, serviceRoleId))
            .ReturnsAsync(response);

        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(_userId, _orgId, serviceRoleId))
            .Returns(true);

        // Act
        var result = await _organisationController.Users(_userId, _orgId, serviceRoleId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        (result?.Value as IQueryable<OrganisationUsersResponseModel>).Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task When_User_List_For_Organisation_Is_Requested_With_Valid_Data_And_User_Is_Not_Authorised_Then_Return_403_Forbidden()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();

        _organisationServiceMock.Setup(service => service.GetUserListForOrganisation(_userId, _orgId, serviceRoleId))
            .ReturnsAsync(response);

        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(_userId, _orgId, serviceRoleId))
            .Returns(false);

        // Act
        var result = await _organisationController.Users(_userId, _orgId, serviceRoleId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task AllUsers_ShouldCallServiceWithCorrectParams()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();
        _organisationServiceMock.Setup(service => service.GetUserListForOrganisation(_userId, _orgId, serviceRoleId))
            .ReturnsAsync(response);
        // Act
        var result = await _organisationController.AllUsers(_userId, _orgId, serviceRoleId) as ObjectResult;
        // Assert
        result.Should().NotBeNull();
        (result?.Value as IQueryable<OrganisationUsersResponseModel>).Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseNumber_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel> { new OrganisationResponseModel { Name = "Test Org" } });

        // Act
        var result = await _organisationController.GetOrganisationsByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<OrganisationResponseModel>>();
    }

    [TestMethod]
    public async Task GetOrganisationsByCompaniesHouseNumber_whenInvalidCompanyHouseNumber_ReturnsNoContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _organisationController.GetOrganisationsByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().BeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetByCompaniesHouseNumber_whenInvalidCompanyHouseNumber_ReturnsNoContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((OrganisationResponseModel)null);

        // Act
        var result = await _organisationController.GetByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().BeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetByCompaniesHouseNumber_whenValidCompanyHouseNumber_ReturnsContent()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new OrganisationResponseModel { Name = "Test Org" });

        // Act
        var result = await _organisationController.GetByCompaniesHouseNumber(It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task OrganisationExternalId_WhenUserValid_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByExternalId(_orgExternalId))
            .ReturnsAsync(new OrganisationDetailModel());

        // Act
        var result = await _organisationController.GetOrganisationByExternalIdAsync(_orgExternalId.ToString()) as ObjectResult;

        // Assert
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
        var result = await _organisationController.GetOrganisationByExternalIdAsync(_orgExternalId.ToString()) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetOrganisationByInviteTokenAsync_returns_nocontent_if_no_org()
    {
        var token = "sometokenstring";
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationNameByInviteTokenAsync(token))
            .ReturnsAsync((ApprovedPersonOrganisationModel)null);

        // Act
        var result = await _organisationController.GetOrganisationByExternalIdAsync(token) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetOrganisationByInviteTokenAsync_returns_org_if_found()
    {
        var token = "sometokenstring";
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationNameByInviteTokenAsync(token))
            .ReturnsAsync(new ApprovedPersonOrganisationModel());

        // Act
        var result = await _organisationController.GetOrganisationByExternalIdAsync(token) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationIdAsync_NoRelationships_ReturnsNoContent()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _organisationServiceMock.Setup(s => s.GetOrganisationRelationshipsByOrganisationId(It.IsAny<Guid>()))
            .ReturnsAsync((OrganisationRelationshipResponseModel)null);

        // Act
        var result = await _organisationController.GetOrganisationRelationshipsByOrganisationIdAsync(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationIdAsync_WithRelationships_ReturnsOk()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var detailModel = new OrganisationRelationshipResponseModel();
        _organisationServiceMock.Setup(s => s.GetOrganisationRelationshipsByOrganisationId(It.IsAny<Guid>()))
            .ReturnsAsync(detailModel);

        // Act
        var result = await _organisationController.GetOrganisationRelationshipsByOrganisationIdAsync(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(detailModel, okResult.Value);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsAsync_ReturnsOk()
    {
        // Arrange
        var page = 1;
        var showPerPage = 20;
        var search = "test";

        var items = new List<RelationshipResponseModel>
        {
            new RelationshipResponseModel
            {
                OrganisationName = "Test1",
                OrganisationNumber = "2345",
                RelationshipType = "Parent",
                CompaniesHouseNumber = "CH123455"
            }
        };

        var model = new PagedOrganisationRelationshipsResponse
        {
            Items = items,
            CurrentPage = page,
            TotalItems = 1,
            PageSize = showPerPage,
            SearchTerms = new List<string> { "Test1", "2345", "CH123455" }
        };

        _organisationServiceMock.Setup(s => s.GetPagedOrganisationRelationships(page, showPerPage, search))
            .ReturnsAsync(model);

        // Act
        var result = await _organisationController.GetOrganisationRelationshipsAsync(page, showPerPage, search);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(model, okResult.Value);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsWithoutPagingAsync_ReturnsOk()
    {
        // Arrange
        var items = new List<RelationshipResponseModel>
        {
            new RelationshipResponseModel
            {
                OrganisationName = "Test1",
                OrganisationNumber = "2345",
                RelationshipType = "Parent",
                CompaniesHouseNumber = "CH123455"
            },

            new RelationshipResponseModel
            {
                OrganisationName = "Test2",
                OrganisationNumber = "678",
                RelationshipType = "Parent",
                CompaniesHouseNumber = "CH123455"
            }
        };

        _organisationServiceMock.Setup(s => s.GetUnpagedOrganisationRelationships())
            .ReturnsAsync(items);

        // Act
        var result = await _organisationController.GetOrganisationRelationshipsWithoutPagingAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(items, ((Microsoft.AspNetCore.Mvc.ObjectResult)result).Value);
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiary_Returns_Ok_With_Org()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 123456 });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "0123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _organisationController.CreateAndAddSubsidiary(new LinkOrganisationRequestModel()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("0123456");
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiary_Returns_400BadRequest_When_Invalid_OrganisationId()
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
        var result = await _organisationController.CreateAndAddSubsidiary(new LinkOrganisationRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("organisation does not exist");
        resultValue.Type.Should().Be("organisation/invalid-externalId");
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiary_Returns_400BadRequest_When_Invalid_UserId()
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
        var result = await _organisationController.CreateAndAddSubsidiary(new LinkOrganisationRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
    }

    [TestMethod]
    public async Task AddSubsidiary_Returns_Ok_With_Org()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123" });

        _organisationServiceMock
            .Setup(service => service.AddOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _organisationController.AddSubsidiary(new SubsidiaryAddRequestModel()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("Ref:123");
    }

    [TestMethod]
    public async Task AddSubsidiary_Returns_400_WhenInvalidChildOrganisationId_With_Org()
    {
        // Arrange
        _organisationServiceMock
            .SetupSequence(service => service.GetOrganisationResponseByExternalId(It.IsAny<Guid>())).ReturnsAsync(
            new OrganisationResponseModel { Id = 123456, ReferenceNumber = "Ref:123" }).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationRelationshipsAsync(
                It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new OrganisationRelationship { Id = 1 });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _organisationController.AddSubsidiary(new SubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("child organisation does not exist");
        resultValue.Type.Should().Be("child organisation/invalid-childOrganisationId");
    }

    [TestMethod]
    public async Task AddSubsidiary_Returns_400BadRequest_When_Invalid_OrganisationId()
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
        var result = await _organisationController.AddSubsidiary(new SubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("organisation does not exist");
        resultValue.Type.Should().Be("organisation/invalid-externalId");
    }

    [TestMethod]
    public async Task AddSubsidiary_Returns_400BadRequest_When_Invalid_UserId()
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
        var result = await _organisationController.AddSubsidiary(new SubsidiaryAddRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
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
        var result = await _organisationController.TerminateSubsidiary(new SubsidiaryTerminateRequestModel()) as ObjectResult;

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
        var result = await _organisationController.TerminateSubsidiary(new SubsidiaryTerminateRequestModel()) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("user does not exist");
        resultValue.Type.Should().Be("user/invalid-userId");
    }

    [TestMethod]
    public async Task TerminateSubsidiary_Returns_400BadRequest_When_Invalid_ChildOrganisationId()
    {
        // Arrange
        var parentOrganisationId = Guid.NewGuid();
        var childOrganisationId = Guid.NewGuid();

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(parentOrganisationId)).ReturnsAsync(new OrganisationResponseModel { Id = 123456 });

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByExternalId(childOrganisationId)).ReturnsAsync((OrganisationResponseModel)null);

        _organisationServiceMock
            .Setup(service => service.AddOrganisationAndOrganisationRelationshipsAsync(
                It.IsAny<OrganisationModel>(), It.IsAny<OrganisationRelationshipModel>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Organisation { ReferenceNumber = "0123456" });

        _userServiceMock.Setup(s => s.GetUserByUserId(It.IsAny<Guid>())).ReturnsAsync(new User { Id = 45678 });

        // Act
        var result = await _organisationController.TerminateSubsidiary(new SubsidiaryTerminateRequestModel { ParentOrganisationId = parentOrganisationId, ChildOrganisationId = childOrganisationId }) as ObjectResult;

        var resultValue = result.Value as ValidationProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        resultValue.Detail.Should().Be("child organisation does not exist");
        resultValue.Type.Should().Be("child organisation/invalid-childOrganisationId");
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
        var result = await _organisationController.TerminateSubsidiary(new SubsidiaryTerminateRequestModel()) as OkResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task ExportDirectProducerSubsidiaries_ReturnsOk_WhenDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedData = new List<ExportOrganisationSubsidiariesResponseModel>
        {
            new() { OrganisationId = "1", SubsidiaryId = null, OrganisationName = "ABC", CompaniesHouseNumber = "CH1", JoinerDate = DateTime.Parse("2025-02-01", CultureInfo.InvariantCulture), ReportingType = "Individual" },
            new() { OrganisationId = "1", SubsidiaryId = "2", OrganisationName = "ABC", CompaniesHouseNumber = "CH2", JoinerDate = DateTime.Parse("2025-02-01", CultureInfo.InvariantCulture), ReportingType = "Individual" }
        };

        _organisationServiceMock
            .Setup(service => service.ExportOrganisationSubsidiaries(organisationId))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _organisationController.ExportDirectProducerSubsidiaries(organisationId);

        // Assert
        result.Should().NotBeNull();
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(expectedData, okResult.Value);
    }

    [TestMethod]
    public async Task ExportDirectProducerSubsidiaries_ReturnsNoContent_WhenNoDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedData = new List<ExportOrganisationSubsidiariesResponseModel>();
        expectedData = null;

        _organisationServiceMock
            .Setup(service => service.ExportOrganisationSubsidiaries(organisationId))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _organisationController.ExportDirectProducerSubsidiaries(organisationId);

        // Assert
        result.Should().NotBeNull();
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task UpdateNationIdByOrganisationId_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var orgnaisation = new OrganisationUpdateModel();
        var userId = Guid.NewGuid();

        _organisationServiceMock.Setup(s =>
            s.UpdateOrganisationDetails(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<OrganisationUpdateModel>())).ReturnsAsync(Core.Models.Result.Result.SuccessResult());

        // Act
        var result = await _organisationController.UpdateNationIdByOrganisationId(
            organisationId,
            orgnaisation,
            userId) as OkResult;

        // Assert
        Assert.IsNotNull(result);
        _organisationServiceMock.Verify(o => o.UpdateOrganisationDetails(
            userId,
            organisationId,
            orgnaisation), Times.Once);
    }

    [TestMethod]
    public async Task UpdateNationIdByOrganisationId_WithNoNationId_ReturnsBadRequest()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _organisationController.UpdateNationIdByOrganisationId(
            organisationId,
            null,
            userId) as BadRequestResult;

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateNationIdByOrganisationId_WithValidParameters_ReturnsErrorResponse()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var orgnaisation = new OrganisationUpdateModel();
        var userId = Guid.NewGuid();

        _organisationServiceMock.Setup(s =>
            s.UpdateOrganisationDetails(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<OrganisationUpdateModel>()))
            .ReturnsAsync(Core.Models.Result.Result.FailedResult("Fail!", HttpStatusCode.InternalServerError));

        // Act
        var result = await _organisationController.UpdateNationIdByOrganisationId(
            organisationId,
            orgnaisation,
            userId);

        // Assert
        Assert.IsInstanceOfType<StatusCodeResult>(result);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((StatusCodeResult)result).StatusCode);

        _organisationServiceMock.Verify(o => o.UpdateOrganisationDetails(
            userId,
            organisationId,
            orgnaisation), Times.Once);
    }

    [TestMethod]
    public async Task GetOrganisationResponseByReferenceNumberAsync_OrganisationNotFound_ReturnsNoContentResult()
    {
        // Arrange
        const string organisationReference = "123456";
        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(organisationReference))
            .ReturnsAsync((OrganisationResponseModel)null);

        // Act
        var result = await _organisationController.GetOrganisationResponseByReferenceNumberAsync(organisationReference) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetOrganisationResponseByReferenceNumberAsync_OrganisationNotFound_ReturnsOkResult()
    {
        // Arrange
        const string organisationReference = "123456";
        const int organisationId = 1234;
        var expectedResult = new OrganisationResponseModel { Id = organisationId, ReferenceNumber = organisationReference };

        _organisationServiceMock
            .Setup(service => service.GetOrganisationResponseByReferenceNumber(organisationReference))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _organisationController.GetOrganisationResponseByReferenceNumberAsync(organisationReference) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var responseModel = result.Value as OrganisationResponseModel;
        responseModel.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task GetOrganisationByExternalIdAsync_ReturnsOk_WhenOrganisationsExist()
    {
        // Arrange
        var request = _fixture.Create<UpdatedProducersRequest>();
        var organisationList = _fixture.Create<List<UpdatedProducersResponseModel>>();

        _organisationServiceMock
            .Setup(service => service.GetUpdatedProducers(request))
            .ReturnsAsync(organisationList);

        // Act
        var result = await _organisationController.GetUpdatedProducers(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeEquivalentTo(organisationList);
    }

    [TestMethod]
    public async Task GetOrganisationByExternalIdAsync_ReturnsNoContent_WhenNoOrganisationsExist()
    {
        // Arrange
        var request = _fixture.Create<UpdatedProducersRequest>();
        _organisationServiceMock
            .Setup(service => service.GetUpdatedProducers(request))
            .ReturnsAsync(new List<UpdatedProducersResponseModel>());

        // Act
        var result = await _organisationController.GetUpdatedProducers(request) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }


    [TestMethod]
    public async Task GetOrganisationNationByExternalId_Returns200Ok()
    {
        var expectedResponse = _fixture.Create<List<OrganisationNationResponseModel>>();
        // Arrange
        var request = _fixture.Create<Guid>();
        _regulatorServiceMock
            .Setup(service => service.GetOrganisationNationsAsync(request))
            .ReturnsAsync(expectedResponse);

        _organisationServiceMock
          .Setup(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()))
          .ReturnsAsync(true);

        // Act
        var result = await _organisationController.GetOrganisationNationByExternalId(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeEquivalentTo(expectedResponse[0].NationCode);

        _organisationServiceMock.Verify(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()), Times.Once);
        _regulatorServiceMock
            .Verify(service => service.GetOrganisationNationsAsync(request), Times.Once);

        _organisationServiceMock.VerifyNoOtherCalls();
        _regulatorServiceMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalId_When_NoOrganisationNationExist_Returns_404NotFound()
    {
        // Arrange
        var request = _fixture.Create<Guid>();
        _regulatorServiceMock
            .Setup(service => service.GetOrganisationNationsAsync(request))
            .ReturnsAsync((List<OrganisationNationResponseModel>)null);

        _organisationServiceMock
          .Setup(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()))
          .ReturnsAsync(true);

        // Act
        var result = await _organisationController.GetOrganisationNationByExternalId(request) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.Value.Should().Be("Organisation not found");
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

        _organisationServiceMock.Verify(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()), Times.Once);
        _regulatorServiceMock
            .Verify(service => service.GetOrganisationNationsAsync(request), Times.Once);

        _organisationServiceMock.VerifyNoOtherCalls();
        _regulatorServiceMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalId_When_NoOrganisationExist_Returns_204NoContent()
    {
        // Arrange
        var request = _fixture.Create<Guid>();

        _organisationServiceMock
       .Setup(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()))
       .ReturnsAsync(false);

        _organisationServiceMock
       .Setup(service => service.IsCSOrganisationValidAsync(It.IsAny<Guid>()))
       .ReturnsAsync(false);

        // Act
        var result = await _organisationController.GetOrganisationNationByExternalId(request) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

        _organisationServiceMock.Verify(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()), Times.Once);
        _regulatorServiceMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalId_When_NoOrganisationExistButCSOrganisationExists_Returns()
    {
        // Arrange
        var expectedResponse = _fixture.Create<List<OrganisationNationResponseModel>>();
        var request = _fixture.Create<Guid>();

        _organisationServiceMock
           .Setup(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()))
           .ReturnsAsync(false);

        _organisationServiceMock
           .Setup(service => service.IsCSOrganisationValidAsync(It.IsAny<Guid>()))
           .ReturnsAsync(true);

        _regulatorServiceMock
            .Setup(service => service.GetCSOrganisationNationsAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _organisationController.GetOrganisationNationByExternalId(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeEquivalentTo(expectedResponse[0].NationCode);

        _organisationServiceMock.Verify(service => service.IsOrganisationValidAsync(It.IsAny<Guid>()), Times.Once);
        _organisationServiceMock.Verify(service => service.IsCSOrganisationValidAsync(It.IsAny<Guid>()), Times.Once);
        _regulatorServiceMock.Verify(service => service.GetCSOrganisationNationsAsync(request), Times.Once);
    }

    [TestMethod]
    public async Task GetPersonEmailsAsync_WhenEmailsExist_ReturnsOk()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var personEmails = new List<PersonEmailResponseModel>
                            {
                                new PersonEmailResponseModel { Email = "email1@testing.com" },
                                new PersonEmailResponseModel { Email = "email2@testing.com" }
                            };

        _organisationServiceMock
            .Setup(service => service.GetPersonEmails(organisationId, "CS")).ReturnsAsync(personEmails);

        // Act
        var result = await _organisationController.GetPersonEmailsAsync(organisationId, "CS") as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeEquivalentTo(personEmails);
    }

    [TestMethod]
    public async Task GetPersonEmailsAsync_WhenNoEmails_ReturnsNoContent()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var personEmails = new List<PersonEmailResponseModel>();

        _organisationServiceMock
            .Setup(service => service.GetPersonEmails(organisationId, "CS"))
            .ReturnsAsync(personEmails);

        // Act
        var result = await _organisationController.GetPersonEmailsAsync(organisationId, "CS") as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetPersonEmailsAsync_WhenBadRequest_ReturnsBadRequest()
    {
        // Arrange
        Guid organisationId = Guid.Empty;

        // Act
        var result = await _organisationController.GetPersonEmailsAsync(organisationId, "CS") as BadRequestResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task ValidateProducerEprId_WhenExternalIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var externalId = Guid.Empty;
        var entityTypeCode = "EntityType";

        // Act
        var result = await _organisationController.ValidateProducerEprId(externalId, entityTypeCode) as BadRequestResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task ValidateProducerEprId_WhenExternalIdExists_ReturnsOk()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var entityTypeCode = "EntityType";

        _validateDataService.Setup(service => service.IsExternalIdExists(externalId, entityTypeCode))
            .Returns(true);

        // Act
        var result = await _organisationController.ValidateProducerEprId(externalId, entityTypeCode) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().Be(externalId);
    }

    [TestMethod]
    public async Task ValidateProducerEprId_WhenExternalIdDoesNotExist_ReturnsNoContent()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var entityTypeCode = "EntityType";

        _validateDataService.Setup(service => service.IsExternalIdExists(externalId, entityTypeCode))
            .Returns(false);

        // Act
        var result = await _organisationController.ValidateProducerEprId(externalId, entityTypeCode) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

	[TestMethod]
	public async Task TeamMembers_ShouldCallService_ReturnsTeamMembers()
	{
		// Arrange
		var response = _fixture.Create<IQueryable<TeamMembersResponseModel>>();
		_organisationServiceMock.Setup(service => service.GetTeamMemberListForOrganisation(_userId, _orgId, serviceRoleId))
			.ReturnsAsync(response);
		// Act
		var result = await _organisationController.TeamMembers(_userId, _orgId, serviceRoleId) as ObjectResult;
		// Assert
		result.Should().NotBeNull();
		(result?.Value as IQueryable<TeamMembersResponseModel>).Should().BeEquivalentTo(response);
	}

	[TestMethod]
	public async Task TeamMembers_ShouldCallService_ReturnsNoTeamMembers()
	{
		// Arrange
		var response = new List<TeamMembersResponseModel>().AsQueryable(); // Ensure the response is IQueryable
		_organisationServiceMock.Setup(service => service.GetTeamMemberListForOrganisation(_userId, _orgId, serviceRoleId))
			.ReturnsAsync(response);

		// Act
		var result = await _organisationController.TeamMembers(_userId, _orgId, serviceRoleId) as ObjectResult;

		// Assert
		result.Should().NotBeNull();
		(result?.Value as IQueryable<TeamMembersResponseModel>).Should().BeEquivalentTo(response);
	}
}