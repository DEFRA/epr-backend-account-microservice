using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers.ReprocessorExporterAccountsControllerTests;

[TestClass]
public class CreateAccountTests
{
    private const string ServiceName = "ServiceName";
    private readonly Guid _userId = Guid.Parse("1d1d1d1d-dead-beef-c0de-133713371337");
    private Guid _userGuid;
    private ReprocessorExporterAccountsController? _reprocessorExporterAccountsController;
    private Mock<IAccountService>? _accountServiceMock;
    private Mock<IPersonService>? _personServiceMock;
    private Mock<IPartnerService>? _partnerServiceMock;
    private Mock<IOrganisationService> _organisationService;
    private Mock<IOptions<ApiConfig>>? _apiConfigOptionsMock;

    [TestInitialize]
    public void Setup()
    {
        _userGuid = Guid.NewGuid();

        _accountServiceMock = new Mock<IAccountService>();
        _personServiceMock = new Mock<IPersonService>();
        _partnerServiceMock = new Mock<IPartnerService>();
        _organisationService = new Mock<IOrganisationService>();

        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _reprocessorExporterAccountsController = new ReprocessorExporterAccountsController(
            _accountServiceMock.Object,
            _personServiceMock.Object,
            _partnerServiceMock.Object,
            _organisationService.Object,
            _apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task CreateAccount_WhenValidAccount_ReturnsOkResult()
    {
        // Arrange
        //todo: move to setup
        var account = GetReprocessorExporterAccount();

        _personServiceMock!
            .Setup(service => service.GetPersonResponseByUserId(_userGuid))
            .ReturnsAsync((PersonResponseModel?)null);

        _accountServiceMock!
            .Setup(service => service.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccount>(), ServiceName, _userId))
            .ReturnsAsync(new Person());

        // Act
        var result = await _reprocessorExporterAccountsController!.CreateAccount(account, ServiceName, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task CreateAccount_WhenValidAccount_CallsAddReprocessorExporterAccountAsync()
    {
        // Arrange
        var account = GetReprocessorExporterAccount();

        _personServiceMock!
            .Setup(service => service.GetPersonResponseByUserId(_userGuid))
            .ReturnsAsync((PersonResponseModel?)null);

        _accountServiceMock!
            .Setup(service => service.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccount>(), ServiceName, _userId))
            .ReturnsAsync(new Person());

        // Act
        await _reprocessorExporterAccountsController!.CreateAccount(account, ServiceName, _userId);

        // Assert
        //todo: could check correct account was passed (easier if we swap to records, but leave as class for now for consistency)
        _accountServiceMock.Verify(x => x.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccount>(), ServiceName, _userId), Times.Once);
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserAlreadyExists_DoesNotCallAddReprocessorExporterAccountAsync()
    {
        // Arrange
        var account = GetReprocessorExporterAccount();

        _personServiceMock!
            .Setup(service => service.GetPersonResponseByUserId(account.User.UserId!.Value))
            .ReturnsAsync(new PersonResponseModel());

        _accountServiceMock!
            .Setup(service => service.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccount>(), ServiceName, _userId))
            .ReturnsAsync(new Person());

        // Act
        await _reprocessorExporterAccountsController!.CreateAccount(account, ServiceName, _userId);

        // Assert
        _accountServiceMock.Verify(x => x.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccount>(), ServiceName, _userId), Times.Never);
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserAlreadyExists_ReturnsCorrectProblem()
    {
        // Arrange
        var account = GetReprocessorExporterAccount();

        _personServiceMock!
            .Setup(service => service.GetPersonResponseByUserId(account.User.UserId!.Value))
            .ReturnsAsync(new PersonResponseModel());

        // Act
        var result = await _reprocessorExporterAccountsController!.CreateAccount(account, ServiceName, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.Value.Should().NotBeNull();
        objectResult!.Value.Should().BeOfType<ValidationProblemDetails>();
        var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
        validationProblemDetails!.Status.Should().Be(409);
        validationProblemDetails!.Detail.Should().Be("User already exists");
        validationProblemDetails!.Type.Should().Be("create-reprocessor-exporter-account/user-exists");
        validationProblemDetails!.Errors.Count.Should().Be(1);
        validationProblemDetails!.Errors.First().Key.Should().Be("UserId");
        validationProblemDetails!.Errors.First().Value.Should().Contain($"User '{_userGuid}' already exists");
    }

    [TestMethod]
    public async Task CreateAccount_WhenProblemDetailsFactoryIsWiredUp_UsesFactoryToBuildProblem()
    {
        // Arrange
        // This test exercises the ApiControllerBase.TypedValidationProblem `else` branch —
        // ProblemDetailsFactory is null in the other tests (DefaultHttpContext), so the
        // fallback "improvise for testability" path runs. Here we supply a factory and
        // verify it's used instead.
        var account = GetReprocessorExporterAccount();
        _personServiceMock!
            .Setup(service => service.GetPersonResponseByUserId(account.User.UserId!.Value))
            .ReturnsAsync(new PersonResponseModel());

        var factoryMock = new Mock<ProblemDetailsFactory>();
        factoryMock
            .Setup(f => f.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .Returns(new ValidationProblemDetails { Status = 409, Detail = "from-factory" });

        _reprocessorExporterAccountsController!.ProblemDetailsFactory = factoryMock.Object;

        // Act
        var result = await _reprocessorExporterAccountsController.CreateAccount(account, ServiceName, _userId);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(409);
        (objectResult.Value as ValidationProblemDetails)!.Detail.Should().Be("from-factory");
        factoryMock.Verify(f => f.CreateValidationProblemDetails(
            It.IsAny<HttpContext>(),
            It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()), Times.Once);
    }

    private ReprocessorExporterAccount GetReprocessorExporterAccount()
    {
        return new ReprocessorExporterAccount
        {
            Person = new PersonModel()
            {
                ContactEmail = "test@example.com",
                FirstName = "Teddy",
                LastName = "Swims",
                TelephoneNumber = "07905606060",
            },
            User = new UserModel()
            {
                UserId = _userGuid,
                Email = "test@example.com"
            }
        };
    }
}
