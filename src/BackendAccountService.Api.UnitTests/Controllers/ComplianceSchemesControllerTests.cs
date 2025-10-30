namespace BackendAccountService.Api.UnitTests.Controllers;

using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models;
using Configuration;
using Core.Models;
using Core.Models.Request;
using Core.Models.Responses;
using Core.Models.Result;
using Core.Services;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Globalization;

[TestClass]
public class ComplianceSchemesControllerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<ILogger<ComplianceSchemesController>> _logger = new();
    private readonly Mock<IComplianceSchemeService> _complianceSchemeServiceMock = new();
    private readonly Mock<IValidationService> _validationService = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private ComplianceSchemesController _complianceSchemeController;
    private readonly Guid _testGuid = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });
        
        _complianceSchemeController = new ComplianceSchemesController(
            _complianceSchemeServiceMock.Object,
            _apiConfigOptionsMock.Object,
            _logger.Object,
            _validationService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }


    [TestMethod]
    public async Task GetComplianceSchemeMembers_UserIsAuthorised_ReturnOk()
    {
        string? query = null;
        int pageSize = 10;
        int page = 1;
        var organisatonId = _fixture.Create<Guid>();
        var complianceSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMembershipResponse>.SuccessResult(_fixture.Create<ComplianceSchemeMembershipResponse>());
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMembersAsync(organisatonId, complianceSchemeId, query, pageSize, page, false)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisatonId)).ReturnsAsync(true);

        var result = await _complianceSchemeController.GetComplianceSchemeMembers(organisatonId, complianceSchemeId, pageSize, userId, query, page) as OkObjectResult;

        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (result?.Value as ComplianceSchemeMembershipResponse).Should().BeEquivalentTo(response.Value);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembers_UserIsNotAuthorised_ReturnForbiddenResponse()
    {
        string? query = null;
        int pageSize = 10;
        int page = 1;
        var organisatonId = _fixture.Create<Guid>();
        var complianceSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMembershipResponse>.SuccessResult(_fixture.Create<ComplianceSchemeMembershipResponse>());
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMembersAsync(organisatonId, complianceSchemeId, query, pageSize, page, false)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisatonId)).ReturnsAsync(false);

        var result = await _complianceSchemeController.GetComplianceSchemeMembers(organisatonId, complianceSchemeId, pageSize, userId, query, page) as ActionResult;

        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembers_ServiceBadRequest_ReturnBadRequest()
    {
        string? query = null;
        int pageSize = 10;
        int page = 1;
        var organisatonId = _fixture.Create<Guid>();
        var complianceSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMembershipResponse>.FailedResult("BadRequest", HttpStatusCode.BadRequest);
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMembersAsync(organisatonId, complianceSchemeId, query, pageSize, page, false)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisatonId)).ReturnsAsync(true);

        var result = await _complianceSchemeController.GetComplianceSchemeMembers(organisatonId, complianceSchemeId, pageSize, userId, query, page) as ActionResult;

        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembers_ServiceComplianceSchemeNotFound_ReturnNotFound()
    {
        string? query = null;
        int pageSize = 10;
        int page = 1;
        var organisatonId = _fixture.Create<Guid>();
        var complianceSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMembershipResponse>.FailedResult("NotFound", HttpStatusCode.NotFound);
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMembersAsync(organisatonId, complianceSchemeId, query, pageSize, page, false)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisatonId)).ReturnsAsync(true);

        var result = await _complianceSchemeController.GetComplianceSchemeMembers(organisatonId, complianceSchemeId, pageSize, userId, query, page) as ActionResult;

        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembers_ExceptionThrownInService_ThrowToGlobalHandler()
    {
        string? query = null;
        int pageSize = 10;
        int page = 1;
        var organisatonId = _fixture.Create<Guid>();
        var complianceSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMembersAsync(organisatonId, complianceSchemeId, query, pageSize, page, false)).ThrowsAsync(new Exception());
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisatonId)).ReturnsAsync(true);

        var result = async () => await _complianceSchemeController.GetComplianceSchemeMembers(organisatonId, complianceSchemeId, pageSize, userId, query, page) as ActionResult;

        await result.Should().ThrowExactlyAsync<Exception>();
    }


    [TestMethod]
    public async Task WhenRemoveComplianceSchemeMemberIsCalled_AndRequestIsCorrect_ThenReturnOk()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test Description"
        };
        var response = Result<RemoveComplianceSchemeMemberResponse>.SuccessResult(_fixture.Create<RemoveComplianceSchemeMemberResponse>());
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();

        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, removeScheme))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        // Act
        var result = await _complianceSchemeController.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, removeScheme) as ActionResult;

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    
    [TestMethod]
    public async Task WhenRemoveComplianceSchemeMemberIsCalled_AndRequestIsIncorrect_ThenReturnError()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test Description"
        };
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var errorResponse = Result<RemoveComplianceSchemeMemberResponse>.FailedResult("BadRequest", HttpStatusCode.BadRequest);

        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, removeScheme))
            .ReturnsAsync(errorResponse);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        // Act
        var result = await _complianceSchemeController.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, removeScheme) as ActionResult;

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_Remove_Compliance_Is_Called_And_User_Is_Authorised_Then_Return_Ok()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _testGuid,
            OrganisationId = _testGuid,
            UserOId = _testGuid
        };
        var response = Result.SuccessResult();

        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeAsync(removeScheme))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(removeScheme.UserOId, removeScheme.OrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.RemoveComplianceScheme(removeScheme) as OkResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
    
    [TestMethod]
    public async Task When_Remove_Compliance_Is_Called_And_User_Is_Not_Authorised_Then_Return_Forbidden_Response()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _testGuid,
            OrganisationId = _testGuid,
            UserOId = _testGuid
        };
        var response = Result.SuccessResult();

        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeAsync(removeScheme))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(removeScheme.UserOId, removeScheme.OrganisationId))
            .Returns(false);

        // Act
        var result = await _complianceSchemeController.RemoveComplianceScheme(removeScheme) as ActionResult;

        // Assert
        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task When_Remove_Compliance_Is_Called_And_User_Is_Authorised_And_Service_Throws_Exception_Should_Throw_Exception_To_Global_Handler()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _testGuid,
            OrganisationId = _testGuid,
            UserOId = _testGuid
        };

        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(removeScheme.UserOId, removeScheme.OrganisationId))
            .Returns(true);
        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeAsync(removeScheme))
            .ThrowsAsync(new Exception());

        // Act
        var result = async () => await _complianceSchemeController.RemoveComplianceScheme(removeScheme) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }
    
    [TestMethod]
    public async Task When_Remove_Compliance_Is_Called_And_User_Is_Authorised_And_Selected_Scheme_Id_Not_Valid_Returns_NotFound()
    {
        // Arrange
        var removeScheme = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _testGuid,
            OrganisationId = _testGuid,
            UserOId = _testGuid
        };
        var response = Result.FailedResult("Failed to remove", HttpStatusCode.NotFound);

        _complianceSchemeServiceMock.Setup(service => service.RemoveComplianceSchemeAsync(removeScheme))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(removeScheme.UserOId, removeScheme.OrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.RemoveComplianceScheme(removeScheme) as ActionResult;

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [TestMethod]
    public async Task When_Select_Compliance_Scheme_Is_Called_And_User_Is_Authorised_Then_Return_Selected_Scheme_Ok()
    {
        // Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOId = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.SuccessResult(_fixture.Create<SelectedScheme>());

        _complianceSchemeServiceMock.Setup(service => service.SelectComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOId, request.ProducerOrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.SelectComplianceScheme(request) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (result?.Value as SelectedSchemeDto).Should().BeEquivalentTo(new SelectedSchemeDto(response.Value.ExternalId));
    }
    
    [TestMethod]
    public async Task When_Select_Compliance_Scheme_Is_Called_And_User_Is_Not_Authorised_Then_Return_Forbidden_Response()
    {
        // Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOId = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.SuccessResult(_fixture.Create<SelectedScheme>());

        _complianceSchemeServiceMock.Setup(service => service.SelectComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOId, request.ProducerOrganisationId))
            .Returns(false);

        // Act
        var result = await _complianceSchemeController.SelectComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task When_Select_Compliance_Scheme_Is_Called_And_User_Is_Authorised_And_Request_Has_validation_Error_Then_Return_Bad_Request()
    {
        // Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOId = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.FailedResult("BadRequest", HttpStatusCode.BadRequest);

        _complianceSchemeServiceMock.Setup(service => service.SelectComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOId, request.ProducerOrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.SelectComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_Select_Compliance_Scheme_Is_Called_And_User_Is_Authorised_And_Organisation_Not_Found_Then_Return_Not_Found_Response()
    {
        // Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOId = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.FailedResult("NotFound", HttpStatusCode.NotFound);

        _complianceSchemeServiceMock.Setup(service => service.SelectComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOId, request.ProducerOrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.SelectComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task When_Select_Compliance_Scheme_Is_Called_And_User_Is_Authorised_AndException_Thrown_In_Service_Then_Throw_To_Global_Handler()
    {
        // Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOId = _fixture.Create<Guid>()
        };

        _complianceSchemeServiceMock.Setup(service => service.SelectComplianceSchemeAsync(request))
            .ThrowsAsync(new Exception());
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOId, request.ProducerOrganisationId))
            .Returns(true);
        
        // Act
        var result = async () => await _complianceSchemeController.SelectComplianceScheme(request) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }
    
    [TestMethod]
    public async Task When_Update_Compliance_Scheme_Is_Requested_And_User_Is_Authorised_Then_Returns_Selected_Scheme_Ok()
    {
        // Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _fixture.Create<Guid>(),
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOid = _fixture.Create<Guid>()
        };
        
        var response = Result<SelectedScheme>.SuccessResult(_fixture.Create<SelectedScheme>());
        
        _complianceSchemeServiceMock.Setup(service => service.UpdateSelectedComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService
            .Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOid, request.ProducerOrganisationId))
            .Returns(true);
        
        // Act
        var result = await _complianceSchemeController.UpdateSelectedComplianceScheme(request) as OkObjectResult;

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (result.Value as SelectedSchemeDto).Should().BeEquivalentTo(new SelectedSchemeDto(response.Value.ExternalId));
    }
    
    [TestMethod]
    public async Task When_Update_Compliance_Scheme_Is_Requested_And_User_Is_Not_Authorised_Then_Returns_Forbidden_Response()
    {
        // Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _fixture.Create<Guid>(),
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOid = _fixture.Create<Guid>()
        };
        
        var response = Result<SelectedScheme>.SuccessResult(_fixture.Create<SelectedScheme>());
        
        _complianceSchemeServiceMock.Setup(service => service.UpdateSelectedComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService
            .Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOid, request.ProducerOrganisationId))
            .Returns(false);
        
        // Act
        var result = await _complianceSchemeController.UpdateSelectedComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task When_Update_Compliance_Scheme_Is_Requested_And_User_Is_Authorised_And_Exception_Thrown_In_Service_Then_Throw_To_Global_Handler()
    {
        // Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _fixture.Create<Guid>(),
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOid = _fixture.Create<Guid>()
        };

        _complianceSchemeServiceMock.Setup(service => service.UpdateSelectedComplianceSchemeAsync(request))
            .ThrowsAsync(new Exception());
        _validationService
            .Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOid, request.ProducerOrganisationId))
            .Returns(true);
        
        // Act
        var result = async () => await _complianceSchemeController.UpdateSelectedComplianceScheme(request) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }
    
    [TestMethod]
    public async Task When_Update_Compliance_Scheme_Is_Requested_And_User_Is_Authorised_And_Request_Has_Validation_Error_Then_Returns_Bad_Request()
    {
        // Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _fixture.Create<Guid>(),
            ComplianceSchemeId = _fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOid = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.FailedResult("BadRequest", HttpStatusCode.BadRequest);

        _complianceSchemeServiceMock.Setup(service => service.UpdateSelectedComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOid, request.ProducerOrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.UpdateSelectedComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_Update_Compliance_Scheme_Is_Requested_And_User_Is_Authorised_And_Organisation_NotFound_Then_Returns_Not_Found()
    {
        // Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _fixture.Create<Guid>(),
            ComplianceSchemeId =_fixture.Create<Guid>(),
            ProducerOrganisationId = _fixture.Create<Guid>(),
            UserOid = _fixture.Create<Guid>()
        };

        var response = Result<SelectedScheme>.FailedResult("NotFound", HttpStatusCode.NotFound);

        _complianceSchemeServiceMock.Setup(service => service.UpdateSelectedComplianceSchemeAsync(request))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(request.UserOid, request.ProducerOrganisationId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.UpdateSelectedComplianceScheme(request) as ActionResult;

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [TestMethod]
    public async Task When_Get_Producer_Compliance_Scheme_Is_Called_And_User_Is_Authorised_Then_Return_Selected_Scheme_Ok()
    {
        // Arrange
        var orgId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        
        var response = Result<ProducerComplianceSchemeDto>.SuccessResult(_fixture.Create<ProducerComplianceSchemeDto>());
        
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeForProducer(orgId))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(userId, orgId))
            .Returns(true);
        
        // Act
        var result = await _complianceSchemeController.GetComplianceSchemeForProducer(userId, orgId) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (result?.Value as ProducerComplianceSchemeDto).Should().BeEquivalentTo(response.Value);
    }
    
    [TestMethod]
    public async Task When_Get_Compliance_Schemes_For_Producer_Is_Called_And_User_Is_Authorised_And_Organisation_Is_Not_Found_Then_Return_Not_Found()
    {
        // Arrange
        var orgId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        
        var response = Result<ProducerComplianceSchemeDto>.FailedResult("NotFound", HttpStatusCode.NotFound);

        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(userId, orgId))
            .Returns(true);
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeForProducer(orgId))
            .ReturnsAsync(response);

        // Act
        var result = await _complianceSchemeController.GetComplianceSchemeForProducer(userId, orgId) as ActionResult;

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task When_Get_Compliance_Schemes_For_Producer_Is_Called_And_User_Is_Authorised_And_Request_Has_Validation_Errors_Then_Returns_Bad_Request()
    {
        // Arrange
        var orgId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();

        var response = Result<ProducerComplianceSchemeDto>.FailedResult("BadRequest", HttpStatusCode.BadRequest);

        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeForProducer(orgId))
            .ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(userId, orgId))
            .Returns(true);

        // Act
        var result = await _complianceSchemeController.GetComplianceSchemeForProducer(userId, orgId) as ActionResult;

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task  When_Get_Compliance_Schemes_For_Producer_Is_Called_And_User_Is_Authorised_And_Exception_Thrown_In_Service_Throws_Exception()
    {
        // Arrange
        var orgId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();

        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeForProducer(orgId))
            .ThrowsAsync(new Exception());
        _validationService.Setup(service => service.IsAuthorisedToManageComplianceScheme(userId, orgId))
            .Returns(true);
        // Act
        var result = async () => await _complianceSchemeController.GetComplianceSchemeForProducer(userId, orgId) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    
    [TestMethod]
    public async Task When_Get_All_Compliance_Schemes_Is_Called_Then_Return_Compliance_Schemes_List()
    {
        // Arrange
        var expectedResponse = _fixture.CreateMany<ComplianceSchemeDto>().ToList();

        _complianceSchemeServiceMock.Setup(service => service.GetAllComplianceSchemesAsync())
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _complianceSchemeController.GetAllComplianceSchemes() as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<ComplianceSchemeDto>>();
        (result?.Value).Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetAllComplianceSchemes_WhenExceptionThrownInService_ShouldNotCatchController()
    {
        //Arrange
        _complianceSchemeServiceMock.Setup(service => service.GetAllComplianceSchemesAsync())
            .ThrowsAsync(new Exception());
        
        // Act
        var result = () => _complianceSchemeController.GetAllComplianceSchemes();

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    [TestMethod]
    public async Task When_Get_Compliance_Schemes_For_Operator_Is_Called_Then_Return_Compliance_Schemes_List()
    {
        // Arrange
        var orgId = _fixture.Create<Guid>();
        var expectedResponse = _fixture.CreateMany<ComplianceSchemeDto>().ToList();

        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemesForOperatorAsync(orgId))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _complianceSchemeController.GetComplianceSchemesForOperator(orgId) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<ComplianceSchemeDto>>();
        (result?.Value).Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task Get_Compliance_Schemes_For_Operator_When_Exception_Thrown_In_Service_Should_Not_Catch_Controller()
    {
        //Arrange
        var orgId = _fixture.Create<Guid>();
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemesForOperatorAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());
        
        // Act
        var result = () => _complianceSchemeController.GetComplianceSchemesForOperator(orgId);

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_UserIsAuthorised_ReturnOk()
    {
        //Arrange
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMemberDetailDto>.SuccessResult(_fixture.Create<ComplianceSchemeMemberDetailDto>());
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMemberDetailsAsync(organisationId,selectedSchemeId )).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        //Act
        var result = await _complianceSchemeController.GetComplianceSchemeMemberDetails(organisationId, selectedSchemeId, userId) as OkObjectResult;

        ///Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (result?.Value as ComplianceSchemeMemberDetailDto).Should().BeEquivalentTo(response.Value);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_ServiceBadRequest_ReturnBadRequest()
    {
        //Arrange
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMemberDetailDto>.FailedResult("BadRequest", HttpStatusCode.BadRequest);
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMemberDetailsAsync(organisationId, selectedSchemeId)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        //Act
        var result = await _complianceSchemeController.GetComplianceSchemeMemberDetails(organisationId, selectedSchemeId, userId) as ActionResult;

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_ServiceComplianceSchemeNotFound_ReturnNotFound()
    {
        //Arrange
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var response = Result<ComplianceSchemeMemberDetailDto>.FailedResult("NotFound", HttpStatusCode.NotFound);
        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeMemberDetailsAsync(organisationId, selectedSchemeId)).ReturnsAsync(response);
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        //Act
        var result = await _complianceSchemeController.GetComplianceSchemeMemberDetails(organisationId, selectedSchemeId, userId) as ActionResult;

        //Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        (result as NotFoundObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_ExceptionThrownInService_ThrowToGlobalHandler()
    {
        //Arrange
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        _complianceSchemeServiceMock
            .Setup(service => service.GetComplianceSchemeMemberDetailsAsync(organisationId, selectedSchemeId))
            .ThrowsAsync(new Exception());
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        //Act
        var result = async () => await _complianceSchemeController.GetComplianceSchemeMemberDetails(organisationId, selectedSchemeId, userId) as ActionResult;

        //Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_UserIsNotAuthorised_ReturnForbiddenResponse()
    {
        //Arrange
        var organisationId = _fixture.Create<Guid>();
        var selectedSchemeId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        _complianceSchemeServiceMock
            .Setup(service => service.GetComplianceSchemeMemberDetailsAsync(organisationId, selectedSchemeId))
            .ThrowsAsync(new Exception());
        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(false);

        //Act
        var result =
            await _complianceSchemeController.GetComplianceSchemeMemberDetails(organisationId, selectedSchemeId, userId)
                as ActionResult;

        //Assert
        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GetComplianceSchemesSummaries_WhenUserIsNotAuthorised_ThenReturnForbiddenStatusCode()
    {
        //Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(false);

        //Act
        var controllerResponse = await _complianceSchemeController.GetComplianceSchemesSummary(userId, organisationId, Guid.NewGuid()) as ObjectResult;

        ///Assert
        controllerResponse.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GetComplianceSchemesSummaries_WhenAuthorisedUserAsksForExistingData_ThenReturnSummaryList()
    {
        //Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        var complianceSchemeId = Guid.NewGuid();

        var serviceResponse = new ComplianceSchemeSummary
        {
            Name = "Compliance Scheme Name",
            Nation = Core.Models.Nation.Wales,
            MemberCount = 123,
            MembersLastUpdatedOn = DateTimeOffset.Now
        };

        _complianceSchemeServiceMock
            .Setup(service => service.GetComplianceSchemeSummary(organisationId, complianceSchemeId))
            .ReturnsAsync(serviceResponse);

        //Act
        var controllerResponse = await _complianceSchemeController.GetComplianceSchemesSummary(userId, organisationId, complianceSchemeId) as OkObjectResult;

        ///Assert
        controllerResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (controllerResponse?.Value as ComplianceSchemeSummary).Should().BeEquivalentTo(serviceResponse);
    }

    [TestMethod]
    public async Task GetComplianceSchemesSummaries_WhenExceptionThrownInService_ThenThrowInController()
    {
        //Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();

        _validationService.Setup(service => service.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId)).ReturnsAsync(true);

        _complianceSchemeServiceMock
            .Setup(service => service.GetComplianceSchemeSummary(organisationId, complianceSchemeId))
            .ThrowsAsync(new Exception());

        //Act
        var result = async () => await _complianceSchemeController.GetComplianceSchemesSummary(userId, organisationId, complianceSchemeId) as ActionResult;

        //Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    [TestMethod]
    public async Task GetComplianceSchemeReasonsForRemoval_ReasonsFound_ReturnOkResultWithListOfReasons()
    {
        // Arrange
        var expectedResponse = _fixture.CreateMany<ComplianceSchemeRemovalReasonResponse>().ToList();

        _complianceSchemeServiceMock.Setup(service => service.GetComplianceSchemeReasonsForRemovalAsync ())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _complianceSchemeController.GetComplianceSchemeReasonsForRemoval() as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<List<ComplianceSchemeRemovalReasonResponse>>();
        (result?.Value).Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ReturnsOk_WhenDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();
        var expectedData = new List<ExportOrganisationSubsidiariesResponseModel>
        {
            new() { OrganisationId = "1", SubsidiaryId = null, OrganisationName = "ABC", CompaniesHouseNumber = "CH1", JoinerDate = DateTime.Parse("2025-02-01", CultureInfo.InvariantCulture), ReportingType = "Individual" },
            new() { OrganisationId = "1", SubsidiaryId = "2", OrganisationName = "ABC", CompaniesHouseNumber = "CH2", JoinerDate = DateTime.Parse("2025-02-01", CultureInfo.InvariantCulture), ReportingType = "Individual" }
        };
        _complianceSchemeServiceMock
            .Setup(service => service.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _complianceSchemeController.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().NotBeNull();
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(expectedData, okResult.Value);
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ReturnsNoContent_WhenNoDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();
        var expectedData = new List<ExportOrganisationSubsidiariesResponseModel>();
        expectedData = null;

        _complianceSchemeServiceMock
            .Setup(service => service.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _complianceSchemeController.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().NotBeNull();
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }
}