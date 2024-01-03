using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class RegulatorControllerTests
{
    private RegulatorsController _regulatorsController = null!;
    private readonly Mock<IRegulatorService> _regulatorServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly Mock<IOrganisationService> _organisationServiceMock = new();
    private readonly NullLogger<RegulatorsController> _nullLogger = new();
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private const int FirstPage = 1;
    private const int ZeroPage = 0;
    private const int HundredPage = 100;
    private const int PageSizeOne = 1;
    private const int TotalCount = 2;
    private const int NationId = 1;
    private const string ApplicationType = "All";
    private readonly string _baseProblemTypePath = "https://dummytest/";

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _regulatorsController = new RegulatorsController(_regulatorServiceMock.Object, _organisationServiceMock.Object,
            _nullLogger, _apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task When_Get_Pending_Applications_Is_Called_And_Input_Is_Valid_Then_Return_Pending_Applications()
    {
        // Arrange
        var enrolments = new List<OrganisationEnrolments>();
        var response = new PaginatedResponse<OrganisationEnrolments>(enrolments, TotalCount, FirstPage, PageSizeOne);
        _regulatorServiceMock.Setup(service =>
                service.GetPendingApplicationsAsync(NationId, FirstPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);

        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);

        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetPendingApplications(_userId, FirstPage, PageSizeOne, null, ApplicationType)
                as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task When_Get_Pending_Applications_Is_Called_And_Page_Number_Is_Zero_Then_Return_400_Bad_Request()
    {
        // Arrange
        var enrolments = new List<OrganisationEnrolments>();
        var response = new PaginatedResponse<OrganisationEnrolments>(enrolments, TotalCount, ZeroPage, PageSizeOne);
        _regulatorServiceMock.Setup(service =>
                service.GetPendingApplicationsAsync(NationId, ZeroPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetPendingApplications(_userId, ZeroPage, PageSizeOne, null, ApplicationType) as
                ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task
        When_Get_Pending_Applications_Is_Called_And_Page_Number_Greater_Than_Last_Page_Then_Return_400_Bad_Request()
    {
        // Arrange
        var enrolments = new List<OrganisationEnrolments>();
        var response =
            new PaginatedResponse<OrganisationEnrolments>(enrolments, TotalCount, HundredPage, PageSizeOne);
        _regulatorServiceMock.Setup(service =>
                service.GetPendingApplicationsAsync(NationId, HundredPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetPendingApplications(_userId, HundredPage, PageSizeOne, null,
                ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Get_Pending_Applications_Is_Called_And_User_Not_Exists_Then_Return_400_Bad_Request()
    {
        // Arrange
        var enrolments = new List<OrganisationEnrolments>();
        var response =
            new PaginatedResponse<OrganisationEnrolments>(enrolments, TotalCount, HundredPage, PageSizeOne);
        _regulatorServiceMock.Setup(service =>
                service.GetPendingApplicationsAsync(NationId, HundredPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(0);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetPendingApplications(_userId, HundredPage, PageSizeOne, null,
                ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Get_Pending_Applications_Is_Called_And_User_Is_Not_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        var enrolments = new List<OrganisationEnrolments>();
        var response =
            new PaginatedResponse<OrganisationEnrolments>(enrolments, TotalCount, HundredPage, PageSizeOne);
        _regulatorServiceMock.Setup(service =>
                service.GetPendingApplicationsAsync(NationId, HundredPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(0);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(false);

        // Act
        var result =
            await _regulatorsController.GetPendingApplications(_userId, HundredPage, PageSizeOne, null,
                ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_Enrolments_Is_Called_And_Regulator_User_Is_Not_Same_Nation_Then_Return_403_Forbidden()
    {
        // Arrange

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(false);

        // Act
        var result =
            await _regulatorsController.GetPendingApplicationsForOrganisation(_userId, _organisationId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_Enrolments_Is_Called_And_Regulator_User_Is_Of_Same_Nation_Then_Return_200_Ok_Respose()
    {
        // Arrange
        var expectedResonse = new ApplicationEnrolmentDetails
        {
            OrganisationId = _organisationId,
            OrganisationType = "Companies House Company",
            OrganisationName = "Test"
        };

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(true);
        _regulatorServiceMock.Setup(service =>
                service.GetOrganisationEnrolmentDetails(_organisationId))
            .ReturnsAsync(expectedResonse);

        // Act
        var result =
            await _regulatorsController.GetPendingApplicationsForOrganisation(_userId, _organisationId) as
                OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<ApplicationEnrolmentDetails>();
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_Enrolments_Is_Called_And_Regulator_User_Id_Is_Empty_Then_Return_400_BadRequest()
    {
        // Act
        var result =
            await _regulatorsController.GetPendingApplicationsForOrganisation(Guid.Empty, _organisationId) as
                ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_Enrolments_Is_Called_And_Organisation_Id_Is_Empty_Then_Return_400_BadRequest()
    {
        // Act
        var result =
            await _regulatorsController.GetPendingApplicationsForOrganisation(_userId, Guid.Empty) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Manage_Enrolments_Is_Called_And_User_Id_Is_Empty_Then_Return_400_BadRequest()
    {
        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = Guid.Empty,
            EnrolmentId = Guid.NewGuid(),
            EnrolmentStatus = "Approved",
            RegulatorComment = string.Empty
        };
        // Act
        var result =
            await _regulatorsController.UpdateEnrolment(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Manage_Enrolments_Is_Called_And_User_Id_Is_From_Different_Nation_Then_Return_403_Forbidden()
    {
        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = Guid.NewGuid(),
            EnrolmentId = Guid.NewGuid(),
            EnrolmentStatus = "Approved",
            RegulatorComment = string.Empty
        };
        // Act
        _organisationServiceMock.Setup(service => service.GetOrganisationIdFromEnrolment(request.EnrolmentId))
            .ReturnsAsync(Guid.NewGuid());

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(false);
        var result =
            await _regulatorsController.UpdateEnrolment(request) as ObjectResult;


        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task
        When_Manage_Enrolments_Is_Called_And_User_Is_Authorised_And_Input_Is_Valid_Then_Return_Ok_No_Content()
    {
        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = _userId,
            EnrolmentId = _organisationId,
            EnrolmentStatus = "Approved",
            RegulatorComment = string.Empty
        };
        // Act
        _organisationServiceMock.Setup(service => service.GetOrganisationIdFromEnrolment(request.EnrolmentId))
            .ReturnsAsync(_organisationId);

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(true);

        _regulatorServiceMock.Setup(service =>
                service.UpdateEnrolmentStatusForUserAsync(_userId, _organisationId, request.EnrolmentId,
                    request.EnrolmentStatus, string.Empty))
            .ReturnsAsync((true, string.Empty));

        var result =
            await _regulatorsController.UpdateEnrolment(request) as ActionResult;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<NoContentResult>();
        (result as NoContentResult)?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task
        When_Manage_Enrolments_Is_Called_And_User_Is_Authorised_And_Input_Is_Invalid_Then_Return_400_Bad_Request()
    {
        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = _userId,
            EnrolmentId = _organisationId,
            EnrolmentStatus = "Approved",
            RegulatorComment = string.Empty
        };
        // Act
        _organisationServiceMock.Setup(service => service.GetOrganisationIdFromEnrolment(request.EnrolmentId))
            .ReturnsAsync(_organisationId);

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(true);

        _regulatorServiceMock.Setup(service =>
                service.UpdateEnrolmentStatusForUserAsync(_userId, _organisationId, request.EnrolmentId,
                    request.EnrolmentStatus, string.Empty))
            .ReturnsAsync((false, "enrolment not found"));

        var result =
            await _regulatorsController.UpdateEnrolment(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Transfer_Enrolments_Is_Called_And_User_Id_Is_Empty_Then_Return_400_BadRequest()
    {
        var request = new OrganisationTransferNationRequest()
        {
            UserId = Guid.Empty,
            OrganisationId = Guid.NewGuid(),
            TransferNationId = 3,
            TransferComments = string.Empty
        };
        // Act
        var result =
            await _regulatorsController.TransferOrganisationNation(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task
        When_Transfer_Enrolments_Is_Called_And_User_Id_Is_From_Different_Nation_Then_Return_403_Forbidden()
    {
        //Arrange
        var request = new OrganisationTransferNationRequest()
        {
            UserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            TransferNationId = 3,
            TransferComments = string.Empty
        };

        // Act
        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(request.UserId, request.OrganisationId))
            .Returns(false);
        var result =
            await _regulatorsController.TransferOrganisationNation(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task
        When_Transfer_Enrolments_Is_Called_And_User_Is_Authorised_And_Input_Is_Valid_Then_Return_Ok_No_Content()
    {
        var request = new OrganisationTransferNationRequest
        {
            UserId = _userId,
            OrganisationId = _organisationId,
            TransferNationId = 3,
            TransferComments = string.Empty
        };
        // Act

        _regulatorServiceMock.Setup(service =>
                service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(true);

        _regulatorServiceMock.Setup(service =>
                service.TransferOrganisationNation(request))
            .ReturnsAsync((true, string.Empty));

        var result =
            await _regulatorsController.TransferOrganisationNation(request) as ActionResult;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<NoContentResult>();
        (result as NoContentResult)?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task
        When_User_List_For_Regulator_Is_Requested_With_Valid_Data_And_User_Is_Authorised_Regulator_Then_Return_User_List_As_Response()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();

        _regulatorServiceMock.Setup(service => service.GetUserListForRegulator(_organisationId, true))
            .ReturnsAsync(response);

        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(true);

        // Act
        var result = await _regulatorsController.Users(_userId, _organisationId, true) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        (result?.Value as IQueryable<OrganisationUsersResponseModel>).Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task
        When_User_List_For_Regulator_Is_Requested_With_Valid_Data_And_User_Is_Not_Authorised_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        var response = _fixture.Create<IQueryable<OrganisationUsersResponseModel>>();

        _regulatorServiceMock.Setup(service => service.GetUserListForRegulator(_organisationId, true))
            .ReturnsAsync(response);

        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(_userId, _organisationId))
            .Returns(false);

        // Act
        var result = await _regulatorsController.Users(_userId, _organisationId, true) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task When_Get_Organisations_by_SearchTerm_Is_Called_And_Input_Is_Valid_Then_Return_OK()
    {
        // Arrange
        var query = "some query";
        var searchResults = new List<OrganisationSearchResult>();
        var response =
            new PaginatedResponse<OrganisationSearchResult>(searchResults, TotalCount, FirstPage, PageSizeOne);

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsBySearchTerm(query, NationId, FirstPage, PageSizeOne))
            .ReturnsAsync(response);

        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);

        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetOrganisationsBySearchTerm(_userId, FirstPage, PageSizeOne, query) as
                OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task
        When_Get_Organisations_by_SearchTerm_Is_Called_And_Page_Number_Is_Zero_Then_Return_400_Bad_Request()
    {
        // Arrange
        var query = "some query";
        var searchResults = new List<OrganisationSearchResult>();
        var response =
            new PaginatedResponse<OrganisationSearchResult>(searchResults, TotalCount, ZeroPage, PageSizeOne);

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsBySearchTerm(query, NationId, PageSizeOne, ZeroPage))
            .ReturnsAsync(response);

        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetOrganisationsBySearchTerm(_userId, ZeroPage, PageSizeOne, query) as
                ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_Get_Organisations_by_SearchTerm_Is_Called_And_User_Not_Exists_Then_Return_400_Bad_Request()
    {
        // Arrange
        var query = "some query";
        var searchResults = new List<OrganisationSearchResult>();
        var response =
            new PaginatedResponse<OrganisationSearchResult>(searchResults, TotalCount, HundredPage, PageSizeOne);
        _organisationServiceMock.Setup(service =>
                service.GetOrganisationsBySearchTerm(query, NationId, PageSizeOne, HundredPage))
            .ReturnsAsync(response);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(0);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        // Act
        var result =
            await _regulatorsController.GetOrganisationsBySearchTerm(_userId, HundredPage, PageSizeOne, query) as
                ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task
        When_Get_Organisations_by_SearchTerm_Is_Called_And_User_Is_Not_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        var query = "some query";
        var searchResults = new List<OrganisationSearchResult>();
        var response =
            new PaginatedResponse<OrganisationSearchResult>(searchResults, TotalCount, HundredPage, PageSizeOne);
        _organisationServiceMock.Setup(service =>
                service.GetOrganisationsBySearchTerm(query, NationId, PageSizeOne, HundredPage))
            .ReturnsAsync(response);

        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(0);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(false);

        // Act
        var result =
            await _regulatorsController.GetOrganisationsBySearchTerm(_userId, HundredPage, PageSizeOne, query) as
                ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task
        When_Get_Organisation_by_OrganisationId_Is_Called_And_OrganisationId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        var userId =  Guid.NewGuid();
        
        // Act
        var result =
            await _regulatorsController.GetOrganisationsByOrganisationId(Guid.Empty, userId) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task
        When_Get_Organisation_by_OrganisationId_Is_Called_And_User_Id_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        var organisationId =  Guid.NewGuid();
        
        // Act
        var result =
            await _regulatorsController.GetOrganisationsByOrganisationId(organisationId,Guid.Empty) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_by_OrganisationId_Is_Called_And_User_Is_Not_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        var organisationGuid = Guid.NewGuid();
        var userId =  Guid.NewGuid();

        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(userId, organisationGuid))
            .Returns(false);
        
        // Act
        var result =
            await _regulatorsController.GetOrganisationsByOrganisationId(organisationGuid, userId) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task
        When_Get_Organisation_by_OrganisationId_Is_Called_And_User_Is_Regulator_Then_Return_200_With_Expected_Data()
    {
        // Arrange
        var organisationGuid = Guid.NewGuid();
        var userId =  Guid.NewGuid();
        CompanySearchDetailsModel objectToReturn = new CompanySearchDetailsModel();
        _regulatorServiceMock.Setup(service => service.GetCompanyDetailsById(organisationGuid)).ReturnsAsync(objectToReturn);
        
        
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(userId, organisationGuid))
            .Returns(true);
        
        // Act
        var result =
            await _regulatorsController.GetOrganisationsByOrganisationId(organisationGuid, userId) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType(typeof(CompanySearchDetailsModel));
    }
    
    [TestMethod]
    public async Task WhenUserCanAccessOrganisation_GetProducerUsersByOrganisationExternalId_ReturnsOk()
    {
        
        // Arrange
        var organisationExternalId = Guid.NewGuid();
        var userId =  Guid.NewGuid();
        
        _organisationServiceMock
            .Setup(service => service.GetProducerUsers(organisationExternalId))
            .ReturnsAsync(new List<OrganisationUserOverviewResponseModel>());
        
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(userId, organisationExternalId))
            .Returns(true);
        
        // Act
        var result = await _regulatorsController.GetUsersByOrganisationExternalId(userId, organisationExternalId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().BeOfType<List<OrganisationUserOverviewResponseModel>>();
    }
    
    [TestMethod]
    public async Task WhenUserCannotAccessOrganisation_GetProducerUsersByOrganisationExternalId_ReturnsForbidden()
    {
        
        // Arrange
        var organisationExternalId = Guid.NewGuid();
        var userId =  Guid.NewGuid();
        
        _organisationServiceMock
            .Setup(service => service.GetProducerUsers(organisationExternalId))
            .ReturnsAsync(new List<OrganisationUserOverviewResponseModel>());
        
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(userId, organisationExternalId))
            .Returns(false);
        
        // Act
        var result = await _regulatorsController.GetUsersByOrganisationExternalId(userId, organisationExternalId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
    
    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_User_Is_Not_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(false);
        
       // Act
       var result =
           await _regulatorsController.RemoveApprovedPerson(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() ) as ObjectResult;
        
       // Assert
       result.Should().NotBeNull();
       result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_User_Id_Is_Null_Then_Return_400_Bad_Request()
    {
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(Guid.Empty,It.IsAny<Guid>(), It.IsAny<Guid>()) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_OrganisationId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(It.IsAny<Guid>(), It.IsAny<Guid>(), Guid.Empty) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_ConnExternalId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);
        
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(It.IsAny<Guid>(), Guid.Empty, It.IsAny<Guid>()) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_RemoveSucceeds_ShouldReturnNoContent()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);
        
        _regulatorServiceMock.Setup(x => x.RemoveApprovedPerson(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((true, string.Empty));
       
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(Guid.NewGuid(), Guid.NewGuid(),Guid.NewGuid()) as ObjectResult;
        
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }
}