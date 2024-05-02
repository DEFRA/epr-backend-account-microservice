using System.Collections.ObjectModel;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;
using ProducerType = BackendAccountService.Core.Models.ProducerType;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class AccountsControllerTests
{
    private AccountsController _accountsController = null!;
    private Mock<IAccountService> _accountServiceMock = null!;
    private Mock<IOrganisationService> _organisationServiceMock = null!;
    private Mock<IPersonService> _personServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = null!;

    private const string ValidServiceRole = "ValidServiceRole";
    private const string InvalidServiceRole = "InvalidServiceRole";

    [TestInitialize]
    public void Setup()
    {
        _accountServiceMock = new Mock<IAccountService>();
        _organisationServiceMock = new Mock<IOrganisationService>();
        _personServiceMock = new Mock<IPersonService>();
        _userServiceMock = new Mock<IUserService>();

        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _accountsController = new AccountsController(_accountServiceMock.Object,
            _personServiceMock.Object,
            _organisationServiceMock.Object,
            _userServiceMock.Object,
            _apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task CreateAccount_WhenValidAccount_ReturnsOk()
    {
        // Arrange
        var account = GetAccountRecord();

        account.Connection.ServiceRole = ValidServiceRole;

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new Collection<OrganisationResponseModel>());
        
        _personServiceMock
            .Setup(service => service.GetPersonByUserIdAsync(Guid.NewGuid()))
            .ReturnsAsync((PersonResponseModel?)null);

        _accountServiceMock
            .Setup(service => service.GetServiceRoleAsync(ValidServiceRole))
            .ReturnsAsync(new ServiceRole { Id = 1 });
        _accountServiceMock
            .Setup(service => service.AddAccountAsync(It.IsAny<AccountModel>(), It.IsAny<ServiceRole>()))
            .ReturnsAsync(new Enrolment
            {
                Connection = new PersonOrganisationConnection
                {
                    Organisation = new Organisation
                    {
                        ExternalId = new Guid("00000000-0000-0000-0000-000000000123"),
                        ReferenceNumber = "100011"
                    }
                }
            });

        // Act
        var result = await _accountsController.CreateAccount(account) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().Be(new CreateAccountResponse
        {
            OrganisationId = new Guid("00000000-0000-0000-0000-000000000123"),
            ReferenceNumber = "100011"
        });
    }

    [TestMethod]
    public async Task CreateAccount_WhenCompanyAlreadyExists_ReturnsProblem()
    {
        var account = GetAccountRecord();

        account.Connection.ServiceRole = ValidServiceRole;

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new Collection<OrganisationResponseModel> { new() });

        _accountServiceMock
            .Setup(service => service.GetServiceRoleAsync(ValidServiceRole))
            .ReturnsAsync(new ServiceRole { Id = 1 });

        _accountServiceMock
            .Setup(service => service.AddAccountAsync(It.IsAny<AccountModel>(), It.IsAny<ServiceRole>()))
            .ReturnsAsync(new Enrolment
            {
                Connection = new PersonOrganisationConnection
                {
                    Organisation = new Organisation
                    {
                        ExternalId = new Guid("00000000-0000-0000-0000-000000000123"),
                        ReferenceNumber = "100011"
                    }
                }
            });

        var result = await _accountsController.CreateAccount(account) as ObjectResult;

        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails?.Type.Should().Be("create-account/organisation-exists");
        validationProblemDetails?.Errors.Count.Should().Be(1);
        validationProblemDetails?.Errors.FirstOrDefault().Key.Should().Be("CompaniesHouseNumber");
        validationProblemDetails?.Errors.FirstOrDefault().Value.Should().Contain($"Organisation with the same Companies House number '{account.Organisation.CompaniesHouseNumber}' already exists");
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserAlreadyExists_ReturnsProblem()
    {
        var account = GetAccountRecord();

        account.Connection.ServiceRole = ValidServiceRole;

        _organisationServiceMock
            .Setup(service => service.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new Collection<OrganisationResponseModel>());
        
        _personServiceMock
            .Setup(service => service.GetPersonByUserIdAsync(account.User.UserId!.Value))
            .ReturnsAsync(new PersonResponseModel());

        _accountServiceMock
            .Setup(service => service.GetServiceRoleAsync(ValidServiceRole))
            .ReturnsAsync(new ServiceRole { Id = 1 });
        _accountServiceMock
            .Setup(service => service.AddAccountAsync(It.IsAny<AccountModel>(), It.IsAny<ServiceRole>()))
            .ReturnsAsync(new Enrolment
            {
                Connection = new PersonOrganisationConnection
                {
                    Organisation = new Organisation
                    {
                        ExternalId = new Guid("00000000-0000-0000-0000-000000000123"),
                        ReferenceNumber = "100011"
                    }
                }
            });

        var result = await _accountsController.CreateAccount(account) as ObjectResult;

        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails?.Type.Should().Be("create-account/user-exists");
        validationProblemDetails?.Errors.Count.Should().Be(1);
        validationProblemDetails?.Errors.FirstOrDefault().Key.Should().Be("UserId");
        validationProblemDetails?.Errors.FirstOrDefault().Value.Should().Contain($"User '{account.User.UserId}' already exists");
    }
    
    [TestMethod]
    public async Task CreateAccount_WhenInvalidServiceRole_ReturnsProblem()
    {
        var account = GetAccountRecord();
        account.Connection.ServiceRole = InvalidServiceRole;

        _personServiceMock
            .Setup(service => service.GetPersonByUserIdAsync(account.User.UserId!.Value))
            .ReturnsAsync(new PersonResponseModel());

        _accountServiceMock.Setup(service => service.GetServiceRoleAsync(InvalidServiceRole))
            .ReturnsAsync((ServiceRole?)null);

        // Act
        var result = await _accountsController.CreateAccount(account) as ObjectResult;

        // Assert
        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails?.Type.Should().Be("create-account/invalid-service-role");
        validationProblemDetails?.Errors.Count.Should().Be(1);
        validationProblemDetails?.Errors.FirstOrDefault().Key.Should().Be("ServiceRole");
        validationProblemDetails?.Errors.FirstOrDefault().Value.Should().Contain($"Service role '{InvalidServiceRole}' does not exist");        
    }
    
    [TestMethod]
    public async Task CreateApprovedUserAccount_with_valid_role_IsOkay()
    {
        // arrange
        var request = new ApprovedUserAccountModel{
            Organisation = new OrganisationModel(),
            Person = new PersonModel(),
            Connection = new ConnectionModel()};
        _accountServiceMock.Setup(m => m.GetServiceRoleAsync(It.IsAny<string>())).ReturnsAsync(new ServiceRole());
        _userServiceMock.Setup(m => m.GetApprovedUserUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UserModel());
        _accountServiceMock.Setup(m => m.AddApprovedUserAccountAsync(
            It.IsAny<ApprovedUserAccountModel>(),
            It.IsAny<ServiceRole>(),
            It.IsAny<UserModel>()))
            .ReturnsAsync(new Enrolment
            {
                Connection = new PersonOrganisationConnection { Organisation = new Organisation() },
                ApprovedPersonEnrolment = new ApprovedPersonEnrolment()
            });
        
        // act
        var result = await _accountsController.CreateApprovedUserAccount(request);
        
        //assert
        result.Should().BeOfType<OkObjectResult>();
    }
    
    [TestMethod]
    public async Task CreateApprovedUserAccount_with_invalid_role_returns_validationProblem()
    {
        // arrange
        var request = new ApprovedUserAccountModel{
            Organisation = new OrganisationModel(),
            Person = new PersonModel(),
            Connection = new ConnectionModel()};
        _accountServiceMock.Setup(m => m.GetServiceRoleAsync(It.IsAny<string>())).ReturnsAsync((ServiceRole)null);
        _accountServiceMock.Setup(m => m.AddApprovedUserAccountAsync(
                It.IsAny<ApprovedUserAccountModel>(),
                It.IsAny<ServiceRole>(),
                It.IsAny<UserModel>()))
            .ReturnsAsync(new Enrolment{Connection = new PersonOrganisationConnection{Organisation = new Organisation()}, ApprovedPersonEnrolment = new ApprovedPersonEnrolment() });
        
        // act
        var result = await _accountsController.CreateApprovedUserAccount(request);
        
        //assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult).Value.Should().BeOfType<ValidationProblemDetails>();
    }

    private AccountModel GetAccountRecord()
    {
        return new AccountModel
        {
            Connection = new ConnectionModel()
            {
                ServiceRole = "ThisNeedsSet",

            },
            Organisation = new OrganisationModel()
            {
                Address = new AddressModel()
                {
                    SubBuildingName = "Sub-building",
                    BuildingName = "Building",
                    BuildingNumber = "123-125",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent-Locality",
                    County = "Test County",
                    Country = "Northern Ireland",
                    Postcode = "BT48 123",
                    Town = "SomeTown"
                },
                CompaniesHouseNumber = "12345",
                Name = "Test company one",
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ProducerType = ProducerType.NotSet
            },
            Person = new PersonModel()
            {
                ContactEmail = "test@test.com",
                FirstName = "Johnny",
                LastName = "Cash",
                TelephoneNumber = "07905606060",
            },
            User = new UserModel()
            {
                UserId = Guid.NewGuid(),
                Email = "test@test.com"
            }
        };
    }
    
}
