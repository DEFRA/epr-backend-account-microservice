using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class RegulatorControllerTests
{
    private RegulatorsController _regulatorsController = null!;
    private readonly Mock<IRegulatorService> _regulatorServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly Mock<IOrganisationService> _organisationServiceMock = new();
    private readonly Mock<IComplianceSchemeService> _complianceSchemeServiceMock = new();
    private readonly Mock<IValidationService> _validationService = new();
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

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _regulatorsController = new RegulatorsController(
            _regulatorServiceMock.Object,
            _organisationServiceMock.Object,
            _validationService.Object,
            _complianceSchemeServiceMock.Object,
            _nullLogger,
            _apiConfigOptionsMock.Object)
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
    public async Task When_Get_Organisations_by_SearchTerm_Is_Called_And_is_indirect_producer_show_compliance_scheme_membership()
    {
        // Arrange
        var query = "some query";
        var searchResults = new List<OrganisationSearchResult>();
        var orgId = _fixture.Create<Guid>();
        var producerComplianceScheme = Result<ProducerComplianceSchemeDto>.SuccessResult(_fixture.Create<ProducerComplianceSchemeDto>());
        searchResults.Add(new OrganisationSearchResult
        {
            ExternalId = orgId,
            OrganisationType = OrganisationSchemeType.InDirectProducer.ToString()
        });
        var response =
            new PaginatedResponse<OrganisationSearchResult>(searchResults, TotalCount, FirstPage, PageSizeOne);

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsBySearchTerm(query, NationId, FirstPage, PageSizeOne))
            .ReturnsAsync(response);

        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);

        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);

        _complianceSchemeServiceMock
            .Setup(service => service.GetComplianceSchemeForProducer(orgId))
            .ReturnsAsync(producerComplianceScheme);

        // Act
        var result =
            await _regulatorsController.GetOrganisationsBySearchTerm(_userId, FirstPage, PageSizeOne, query) as
                OkObjectResult;

        // Assert
        (result.Value as PaginatedResponse<OrganisationSearchResult>).Should().NotBeNull();
        (result.Value as PaginatedResponse<OrganisationSearchResult>).Items.FirstOrDefault().MemberOfComplianceSchemeName.Should().Be(producerComplianceScheme.Value.ComplianceSchemeName);
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task
        When_Get_Organisation_by_OrganisationId_Is_Called_And_OrganisationId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        var userId = Guid.NewGuid();

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
        var organisationId = Guid.NewGuid();

        // Act
        var result =
            await _regulatorsController.GetOrganisationsByOrganisationId(organisationId, Guid.Empty) as ObjectResult;

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
        var userId = Guid.NewGuid();

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
        var userId = Guid.NewGuid();
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
        var userId = Guid.NewGuid();

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
        var userId = Guid.NewGuid();

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
    public async Task When_RemoveApprovedPerson_WithoutPromotingAnotherAP_Is_Called_And_User_Is_Not_Regulator_Then_Return_403_Forbidden()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(false);

        var request = new ApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            PromotedPersonExternalId = Guid.Empty
        };
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_WithoutPromotingAnotherAP_Is_Called_And_OrganisationId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(false);

        var request = new ApprovedUserRequest
        {
            OrganisationId = Guid.Empty,
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            PromotedPersonExternalId = Guid.Empty
        };
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_Both_RemovedConnectionExternalId_And_PromotePersonExternalId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        var request = new ApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.Empty,
            PromotedPersonExternalId = Guid.Empty
        };
        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_And_Both_RemovedConnectionExternalId_And_PromotedPersonExternalId_Is_Null_Then_Return_400_Bad_Request()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        var request = new ApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.Empty,
            PromotedPersonExternalId = Guid.Empty
        };

        _regulatorServiceMock.Setup(x => x.RemoveApprovedPerson(request))
            .ReturnsAsync(new List<AssociatedPersonResponseModel>());

        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_RemoveApprovedPerson_Is_Called_With_Both_RemovedConnectionExternalId_And_PromotedPersonExternalId_And_RemoveSucceeds_ShouldReturnNoContent()
    {
        // Arrange

        _regulatorServiceMock
              .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
              .Returns(true);

        var request = new ApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            PromotedPersonExternalId = Guid.NewGuid()
        };

        _regulatorServiceMock.Setup(x => x.RemoveApprovedPerson(request))
            .ReturnsAsync(new List<AssociatedPersonResponseModel>());

        // Act
        var result =
            await _regulatorsController.RemoveApprovedPerson(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    [DynamicData(nameof(AddRemoveApprovedUserInvalidInputTestData))]
    public async Task When_AddRemoveApprovedUser_Is_Called_And_InvalidInput_ShouldReturnBadRequest(
        Guid addingOrRemovingUserId, Guid organisationId)
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisationId,
            RemovedConnectionExternalId = Guid.NewGuid(),
            AddingOrRemovingUserId = addingOrRemovingUserId,
            AddingOrRemovingUserEmail = "test1@email.com",
            InvitedPersonEmail = "test2@email.com"
        };

        // Act
        var result =
            await _regulatorsController.AddRemoveApprovedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_AddRemoveApprovedUser_Is_Called_And_UserIsAlreadyInvited_ShouldReturnBadRequest()
    {
        var invitedUserEmail = "test2@email.com";
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        _validationService
            .Setup(x => x.IsUserInvitedAsync(invitedUserEmail))
            .ReturnsAsync(true);

        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            AddingOrRemovingUserId = Guid.NewGuid(),
            AddingOrRemovingUserEmail = "test1@email.com",
            InvitedPersonEmail = invitedUserEmail
        };

        // Act
        var result =
            await _regulatorsController.AddRemoveApprovedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_AddRemoveApprovedUser_Is_Called_And_DataIsCreatedCorrectly_ShouldReturnOk()
    {
        // Arrange
        _regulatorServiceMock
            .Setup(service => service.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        _validationService
            .Setup(x => x.IsUserInvitedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _regulatorServiceMock
            .Setup(x => x.AddRemoveApprovedPerson(It.IsAny<AddRemoveApprovedUserRequest>()))
            .ReturnsAsync(new AddRemoveApprovedPersonResponseModel());

        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            AddingOrRemovingUserId = Guid.NewGuid(),
            AddingOrRemovingUserEmail = "test1@email.com",
            InvitedPersonEmail = "test2@email.com"
        };

        // Act
        var result =
            await _regulatorsController.AddRemoveApprovedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(externalId, userId, organisationId, null);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problemDetails = objectResult.Value as ProblemDetails;
        Assert.IsNotNull(problemDetails);
        Assert.AreEqual("https://dummytest/accept-or-reject-user-change-details-empty", problemDetails.Type);
        Assert.AreEqual("accept or reject user details change request cannot be null", problemDetails.Title);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ReturnsOk_ForAnAcceptedRequest()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var request = new ManageUserDetailsChangeRequest
        {
            HasRegulatorAccepted = true,
            RegulatorComment = ""
        };

        var response = new RegulatorUserDetailsUpdateResponse
        {
            HasUserDetailsChangeAccepted = true,
            HasUserDetailsChangeRejected = false,
            ChangeHistory = new ChangeHistoryModel
            {
                ExternalId = externalId
            }
        };

        _regulatorServiceMock
            .Setup(x => x.IsRegulator(userId))
            .Returns(true);

        var successResult = Result<RegulatorUserDetailsUpdateResponse>.SuccessResult(response);

        _regulatorServiceMock
            .Setup(service => service.AcceptOrRejectUserDetailsChangeRequestAsync(It.IsAny<ManageUserDetailsChangeModel>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(
            externalId,
            userId,
            organisationId,
            request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(response, okResult.Value);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ReturnsOk_ForARejectedRequest()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var request = new ManageUserDetailsChangeRequest
        {
            HasRegulatorAccepted = false,
            RegulatorComment = "Some comment",
        };

        var response = new RegulatorUserDetailsUpdateResponse
        {
            HasUserDetailsChangeAccepted = false,
            HasUserDetailsChangeRejected = true,
            ChangeHistory = new ChangeHistoryModel
            {
                ExternalId = externalId
            }
        };

        _regulatorServiceMock
            .Setup(x => x.IsRegulator(userId))
            .Returns(true);

        var successResult = Result<RegulatorUserDetailsUpdateResponse>.SuccessResult(response);

        _regulatorServiceMock
            .Setup(service => service.AcceptOrRejectUserDetailsChangeRequestAsync(It.IsAny<ManageUserDetailsChangeModel>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(externalId, userId, organisationId, request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(response, okResult.Value);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ReturnsForbidden_ForAForbiddenRequest()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var request = new ManageUserDetailsChangeRequest
        {
            HasRegulatorAccepted = false,
            RegulatorComment = "Some comment",
        };

        var approvalRequest = new ManageUserDetailsChangeModel
        {
            ChangeHistoryExternalId = externalId,
            OrganisationId = organisationId,
            HasRegulatorAccepted = false,
            RegulatorComment = "Some comment",
            UserId = userId
        };

        _regulatorServiceMock.Setup(service =>
            service.IsRegulator(userId))
            .Returns(false);

        var forbiddenResult = Result<RegulatorUserDetailsUpdateResponse>.FailedResult("Forbidden access", HttpStatusCode.Forbidden);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestAsync(approvalRequest))
            .ReturnsAsync(forbiddenResult);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(externalId, userId, organisationId, request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ReturnsForbidden_WhenAcceptOrRejectUserDetailsChangeRequestAsyncReturnsForbidden()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var expectedStatus = StatusCodes.Status403Forbidden;

        var request = new ManageUserDetailsChangeRequest
        {
            HasRegulatorAccepted = false,
            RegulatorComment = "Some comment",
        };

        var approvalRequest = new ManageUserDetailsChangeModel
        {
            ChangeHistoryExternalId = externalId,
            OrganisationId = organisationId,
            HasRegulatorAccepted = false,
            RegulatorComment = "Some comment",
            UserId = userId
        };

        _regulatorServiceMock.Setup(service =>
          service.IsRegulator(userId))
            .Returns(false);

        var forbiddenResult = Result<RegulatorUserDetailsUpdateResponse>.FailedResult("Forbidden access", HttpStatusCode.Forbidden);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestAsync(approvalRequest))
            .ReturnsAsync(forbiddenResult);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(externalId, userId, organisationId, request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(expectedStatus, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequest_LogsErrorAndReturnsErrorResponse_ForErrorHandling()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var errorMessage = "accept or reject user details change request cannot be null";

        var approvalRequest = new ManageUserDetailsChangeModel
        {
            ChangeHistoryExternalId = externalId,
            OrganisationId = organisationId,
            HasRegulatorAccepted = false,
            RegulatorComment = "",
            UserId = userId
        };

        var errorResponse = Result<RegulatorUserDetailsUpdateResponse>.FailedResult(errorMessage, HttpStatusCode.BadRequest);

        _regulatorServiceMock.Setup(service =>
           service.IsRegulator(userId))
            .Returns(true);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestAsync(approvalRequest))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequest(externalId, userId, organisationId, null);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var problemDetailsValue = objectResult.Value as ProblemDetails;
        Assert.AreEqual(errorMessage, problemDetailsValue.Title);
    }

    [TestMethod]
    public async Task UpdateNonCompaniesHouseCompanyByService_ReturnsBadRequest_WhenTheRequestIsNull()
    {
        // Act
        var result = await _regulatorsController.UpdateNonCompaniesHouseCompanyByService(null);

        // Assert
        var badRequestResult = result as ObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        _regulatorServiceMock.Verify(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(null),
            Times.Never());
    }

    [TestMethod]
    public async Task UpdateNonCompaniesHouseCompanyByService_ReturnsOk_ForASuccessfulUpdate()
    {
        // Arrange
        var request = new ManageNonCompaniesHouseCompanyByService
        {
            OrganisationReference = ""
        };

        var expectedResponse = new RegulatorOrganisationUpdateResponse();

        var response = new Result<RegulatorOrganisationUpdateResponse>(true, expectedResponse, "", HttpStatusCode.OK);

        _regulatorServiceMock.Setup(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.UpdateNonCompaniesHouseCompanyByService(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);

        _regulatorServiceMock.Verify(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task UpdateNonCompaniesHouseCompanyByService_ReturnsForbidden_WhenResultIsForbidden()
    {
        // Arrange
        var request = new ManageNonCompaniesHouseCompanyByService
        {
            OrganisationReference = "12345678"
        };

        var expectedResponse = new RegulatorOrganisationUpdateResponse();

        var response = new Result<RegulatorOrganisationUpdateResponse>(false, expectedResponse, "", HttpStatusCode.Forbidden);

        _regulatorServiceMock.Setup(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.UpdateNonCompaniesHouseCompanyByService(request);

        // Assert
        var statusCodeResult = result as StatusCodeResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);

        _regulatorServiceMock.Verify(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task UpdateNonCompaniesHouseCompanyByService_ReturnsErrorResponse_WhenUpdateFailed()
    {
        // Arrange
        var request = new ManageNonCompaniesHouseCompanyByService
        {
            OrganisationReference = "12345678"
        };

        var errorMessage = "An error occurred";

        var expectedResponse = new RegulatorOrganisationUpdateResponse();

        var response = new Result<RegulatorOrganisationUpdateResponse>(false, expectedResponse, errorMessage, HttpStatusCode.BadRequest);

        _regulatorServiceMock.Setup(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.UpdateNonCompaniesHouseCompanyByService(request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(errorMessage, objectResult.Value);

        _regulatorServiceMock.Verify(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_ReturnsBadRequest_WhenTheRequestIsNull()
    {
        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequestByService(null);

        // Assert
        var badRequestResult = result as ObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        _regulatorServiceMock.Verify(service =>
            service.UpdateNonCompaniesHouseCompanyByServiceAsync(null),
            Times.Never());
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_ReturnsOk_ForASuccessfulUpdate()
    {
        // Arrange
        var request = new ManageUserDetailsChangeRequestByService
        {
            OrganisationReference = "12345678"
        };

        var expectedResponse = new RegulatorUserDetailsUpdateByServiceResponse();

        var response = new Result<RegulatorUserDetailsUpdateByServiceResponse>(true, expectedResponse, "", HttpStatusCode.OK);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequestByService(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);

        _regulatorServiceMock.Verify(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_ReturnsForbidden_WhenResultIsForbidden()
    {
        // Arrange
        var request = new ManageUserDetailsChangeRequestByService
        {
            OrganisationReference = "12345678"
        };

        var expectedResponse = new RegulatorUserDetailsUpdateByServiceResponse();

        var response = new Result<RegulatorUserDetailsUpdateByServiceResponse>(false, expectedResponse, "", HttpStatusCode.Forbidden);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequestByService(request);

        // Assert
        var statusCodeResult = result as StatusCodeResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);

        _regulatorServiceMock.Verify(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_ReturnsErrorResponse_WhenUpdateFailed()
    {
        // Arrange
        var request = new ManageUserDetailsChangeRequestByService
        {
            OrganisationReference = "12345678"
        };

        var errorMessage = "An error occurred";

        var expectedResponse = new RegulatorUserDetailsUpdateByServiceResponse();

        var response = new Result<RegulatorUserDetailsUpdateByServiceResponse>(false, expectedResponse, errorMessage, HttpStatusCode.BadRequest);

        _regulatorServiceMock.Setup(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.AcceptOrRejectUserDetailsChangeRequestByService(request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(errorMessage, objectResult.Value);

        _regulatorServiceMock.Verify(service =>
            service.AcceptOrRejectUserDetailsChangeRequestByServiceAsync(request),
            Times.Once());
    }

    [TestMethod]
    public async Task When_GetPendingUserDetailChangeRequests_Is_Called_And_User_Is_Not_Authorised_Then_Return_Forbidden()
    {
        // Arrange
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(false);

        // Act
        var result = await _regulatorsController.GetPendingUserDetailChangeRequests(_userId, FirstPage, PageSizeOne, null, ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task When_GetPendingUserDetailChangeRequests_Is_Called_With_Invalid_Input_Then_Return_BadRequest()
    {
        // Arrange
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(0);

        // Act
        var result = await _regulatorsController.GetPendingUserDetailChangeRequests(_userId, ZeroPage, PageSizeOne, null, ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_GetPendingUserDetailChangeRequests_Is_Called_And_Page_Is_Out_Of_Range_Then_Return_BadRequest()
    {
        // Arrange
        var response = new PaginatedResponse<OrganisationUserDetailChangeRequest>(
            new List<OrganisationUserDetailChangeRequest>(), 1, FirstPage, PageSizeOne);

        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);
        _regulatorServiceMock.Setup(service => service.GetPendingUserDetailChangeRequestsAsync(NationId, HundredPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.GetPendingUserDetailChangeRequests(_userId, HundredPage, PageSizeOne,
            null, ApplicationType) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task When_GetPendingUserDetailChangeRequests_Is_Called_With_Valid_Input_Then_Return_Ok()
    {
        // Arrange
        var response = new PaginatedResponse<OrganisationUserDetailChangeRequest>(new List<OrganisationUserDetailChangeRequest>(), TotalCount, FirstPage, PageSizeOne);
        _regulatorServiceMock.Setup(service => service.IsRegulator(_userId)).Returns(true);
        _regulatorServiceMock.Setup(service => service.GetRegulatorNationId(_userId)).Returns(NationId);
        _regulatorServiceMock.Setup(service => service.GetPendingUserDetailChangeRequestsAsync(NationId, FirstPage, PageSizeOne, null, ApplicationType))
            .ReturnsAsync(response);

        // Act
        var result = await _regulatorsController.GetPendingUserDetailChangeRequests(_userId, FirstPage, PageSizeOne, null, ApplicationType) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task When_GetUserDetailChangeRequest_Is_Called_With_Valid_Input_Then_Return_Ok()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var userDetailChangeRequest = new ChangeHistoryModel();
        var expectedResult = Result<ChangeHistoryModel>.SuccessResult(userDetailChangeRequest);

        // Mock the service to return the expected result
        _regulatorServiceMock
            .Setup(x => x.DoesRegulatorNationMatchOrganisationNation(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(true);

        _regulatorServiceMock
            .Setup(service => service.GetUserDetailChangeRequestAsync(externalId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _regulatorsController.GetUserDetailChangeRequest(_userId, _organisationId, externalId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().Be(expectedResult); // Ensure the whole Result<ChangeHistoryModel> is returned
    }

    [TestMethod]
    public async Task When_GetUserDetailChangeRequest_Is_Called_And_Exception_Is_Thrown_Then_Return_InternalServerError()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        _regulatorServiceMock.Setup(service => service.GetUserDetailChangeRequestAsync(externalId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _regulatorsController.GetUserDetailChangeRequest(_userId, _organisationId, externalId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    private static IEnumerable<object[]> AddRemoveApprovedUserInvalidInputTestData
    {
        get
        {
            yield return new object[] { Guid.NewGuid(), Guid.Empty };
            yield return new object[] { Guid.Empty, Guid.NewGuid() };
        }
    }

}