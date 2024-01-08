using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class OrganisationControllerTests
{
    private OrganisationsController _organisationController = null!;
    private  Mock<IOrganisationService> _organisationServiceMock = null!;
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
        _organisationController = new OrganisationsController(_organisationServiceMock.Object, 
            _apiConfigOptionsMock.Object, _nullLogger, _validateDataService.Object)
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
        
        _organisationServiceMock.Setup(service => service.GetUserListForOrganisation(_userId,_orgId, serviceRoleId))
            .ReturnsAsync(response);
        
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(_userId, _orgId, serviceRoleId))
            .Returns(true);

        // Act
        var result = await _organisationController.Users(_userId,_orgId, serviceRoleId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        (result?.Value as IQueryable<OrganisationUsersResponseModel>).Should().BeEquivalentTo(response);
    }
    
    [TestMethod]
    public async Task When_User_List_For_Organisation_Is_Requested_With_Valid_Data_And_User_Is_Not_Authorised_Then_Return_403_Forbidden()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();
        
        _organisationServiceMock.Setup(service => service.GetUserListForOrganisation(_userId,_orgId, serviceRoleId))
            .ReturnsAsync(response);
        
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(_userId, _orgId, serviceRoleId))
            .Returns(false);

        // Act
        var result = await _organisationController.Users(_userId,_orgId, serviceRoleId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task OrganisationExternalId_WhenUserValid_ReturnsOk()
    {
        // Arrange
        _organisationServiceMock
            .Setup(service => service.GetOrganisationByExternalId(_orgExternalId))
            .ReturnsAsync(new OrganisationDetailModel());
        
        // Act
        var result = await _organisationController.GetOrganisationByExternalIdAsync(_orgExternalId) as ObjectResult;

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
        var result = await _organisationController.GetOrganisationByExternalIdAsync(_orgExternalId) as NoContentResult;

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
}