using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace BackendAccountService.Api.UnitTests.Controllers.ReprocessorExporterAccountsControllerTests;

[TestClass]
public class AddOrganisationTests
{
    private const string ServiceKey = "testServiceKey";

    private Mock<IAccountService> _mockAccountService;
    private Mock<IPersonService> _mockPersonService;
    private Mock<IPartnerService> _mockPartnerService;
    private Mock<IOrganisationService> _mockOrganisationService;
    private Mock<IOptions<ApiConfig>> _mockApiConfigOptions;
    private ReprocessorExporterAccountsController _controller;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockAccountService = new Mock<IAccountService>();
        _mockPersonService = new Mock<IPersonService>();
        _mockPartnerService = new Mock<IPartnerService>();
        _mockOrganisationService = new Mock<IOrganisationService>();
        _mockApiConfigOptions = new Mock<IOptions<ApiConfig>>();

        _mockAccountService.Setup(a => a.GetServiceRoleAsync(Data.DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Key))
            .ReturnsAsync(new ServiceRole
            {
                Id = Data.DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id
            });

        // Setup a default ApiConfig
        var apiConfig = new ApiConfig { BaseProblemTypePath = "/test-problems/" };
        _mockApiConfigOptions.Setup(o => o.Value).Returns(apiConfig);

        _controller = new ReprocessorExporterAccountsController(
            _mockAccountService.Object,
            _mockPersonService.Object,
            _mockPartnerService.Object,
            _mockOrganisationService.Object,
            _mockApiConfigOptions.Object
        );
    }

    private static ReprocessorExporterAddOrganisation CreateValidOrganisationInput(Guid userId)
    {
        return new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel { UserId = userId, IsApprovedUser = true, JobTitle = "Test User" },
            Organisation = new ReprocessorExporterOrganisationModel
            {
                Name = "Test Org",
                OrganisationType = Core.Models.OrganisationType.CompaniesHouseCompany,
                CompaniesHouseNumber = "12345678",
                Address = new AddressModel { Street = "123 Test St", Town = "Testville", Postcode = "T1 EST" },
                Nation = Core.Models.Nation.England
            },
            Partners = [],
            InvitedApprovedUsers = [],
            DeclarationTimeStamp = DateTime.UtcNow
        };
    }

    [TestMethod]
    public async Task AddOrganisation_ValidInput_ReturnsOkWithResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        var mockResponse = new ReExAddOrganisationResponse
        {
            UserFirstName = "Test",
            UserLastName = "User",
            UserServiceRoles = new List<ServiceRoleResponse>
            {
                new ServiceRoleResponse
                {
                    Key = "test-key",
                    Name = "Test Role",
                    Description = "Test Description"
                }
            },
            OrganisationId = Guid.NewGuid(),
            ReferenceNumber = "REF123",
            InvitedApprovedUsers = new List<InvitedApprovedUserResponse>
            {
                new InvitedApprovedUserResponse
                {
                    Email = "invite@test.com",
                    InviteToken = "token123",
                    ServiceRole = new ServiceRoleResponse
                    {
                        Key = "approved-person",
                        Name = "Approved Person",
                        Description = "Approved Person Role"
                    }
                }
            }
        };

        _mockAccountService.Setup(s => s.AddReprocessorExporterOrganisationAsync(
                organisationInput, existingPerson, partnerRolesDict, ServiceKey, userId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value.Should().BeOfType<ReExAddOrganisationResponse>().Subject;
        response.UserFirstName.Should().Be("Test");
        response.UserLastName.Should().Be("User");
        response.ReferenceNumber.Should().Be("REF123");
        response.OrganisationId.Should().Be(mockResponse.OrganisationId);
        response.InvitedApprovedUsers.Should().HaveCount(1);
        response.InvitedApprovedUsers.First().Email.Should().Be("invite@test.com");
        response.InvitedApprovedUsers.First().InviteToken.Should().Be("token123");
    }

    [TestMethod]
    public async Task AddOrganisation_ValidInput_CallsAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        var mockResponse = new ReExAddOrganisationResponse
        {
            UserFirstName = "Test",
            UserLastName = "User",
            UserServiceRoles = new List<ServiceRoleResponse>
            {
                new ServiceRoleResponse
                {
                    Key = "test-key",
                    Name = "Test Role",
                    Description = "Test Description"
                }
            },
            OrganisationId = Guid.NewGuid(),
            ReferenceNumber = "REF123",
            InvitedApprovedUsers = new List<InvitedApprovedUserResponse>
            {
                new InvitedApprovedUserResponse
                {
                    Email = "invite@test.com",
                    InviteToken = "token123",
                    ServiceRole = new ServiceRoleResponse
                    {
                        Key = "approved-person",
                        Name = "Approved Person",
                        Description = "Approved Person Role"
                    }
                }
            }
        };

        _mockAccountService.Setup(s => s.AddReprocessorExporterOrganisationAsync(
                organisationInput, existingPerson, partnerRolesDict, ServiceKey, userId))
            .ReturnsAsync(mockResponse);

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
            organisationInput, existingPerson, partnerRolesDict, ServiceKey, userId), Times.Once);
    }

    [TestMethod]
    public async Task AddOrganisation_UserIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var userIdInBody = Guid.Parse("f2a379ac-766f-4f0d-90b0-02773b19b4a5");
        var auditUserIdInHeader = Guid.Parse("913005f3-10f2-4214-8ebd-e74e0c2aae6f"); // different
        var organisationInput = CreateValidOrganisationInput(userIdInBody);

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userIdInBody)).ReturnsAsync(new Person()); // Needs some person for early flow

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, auditUserIdInHeader);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = result as BadRequestObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        AssertValidationProblemDetails(objectResult.Value,
            "UserId doesn't match audit UserId",
            "UserId",
            "UserId 'f2a379ac-766f-4f0d-90b0-02773b19b4a5' doesn't match audit UserId '913005f3-10f2-4214-8ebd-e74e0c2aae6f'",
            "add-organisation/user-id-mismatch");
    }

    [TestMethod]
    public async Task AddOrganisation_UserIdMismatch_DoesNotCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        var userIdInBody = Guid.Parse("f2a379ac-766f-4f0d-90b0-02773b19b4a5");
        var auditUserIdInHeader = Guid.Parse("913005f3-10f2-4214-8ebd-e74e0c2aae6f"); // different
        var organisationInput = CreateValidOrganisationInput(userIdInBody);

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userIdInBody)).ReturnsAsync(new Person()); // Needs some person for early flow

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, auditUserIdInHeader);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
                It.IsAny<ReprocessorExporterAddOrganisation>(),
                It.IsAny<Person>(),
                It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task AddOrganisation_CompaniesHouseNumberExists_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId); // CHN is "12345678" by default here

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(new Person());

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(organisationInput.Organisation.CompaniesHouseNumber))
            .ReturnsAsync(new List<OrganisationResponseModel> { new() }); // CHN exists

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        AssertValidationProblemDetails(objectResult.Value,
            "Organisation already exists",
            "CompaniesHouseNumber",
            "Organisation with the same Companies House number '12345678' already exists",
            "add-organisation/organisation-exists");

        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
            It.IsAny<ReprocessorExporterAddOrganisation>(),
            It.IsAny<Person>(),
            It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task AddOrganisation_CompaniesHouseNumberExists_DoesNotCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId); // CHN is "12345678" by default here

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(new Person());

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(organisationInput.Organisation.CompaniesHouseNumber))
            .ReturnsAsync(new List<OrganisationResponseModel> { new() }); // CHN exists

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
            It.IsAny<ReprocessorExporterAddOrganisation>(),
            It.IsAny<Person>(),
            It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task AddOrganisation_PersonDoesNotExist_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.Parse("64e3b72c-e000-466f-bf29-89e72996a1a4");
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null; // Pass CHN validation

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync((Person)null); // Person does not exist
        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        AssertValidationProblemDetails(objectResult.Value,
            "Person does not exist",
            "UserId",
            "Person with UserId '64e3b72c-e000-466f-bf29-89e72996a1a4' does not exist",
            "add-organisation/person-does-not-exist");
    }

    [TestMethod]
    public async Task AddOrganisation_PersonDoesNotExist_DoesNotCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        var userId = Guid.Parse("64e3b72c-e000-466f-bf29-89e72996a1a4");
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null; // Pass CHN validation

        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(ImmutableDictionary<string, PartnerRole>.Empty);
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync((Person)null); // Person does not exist
        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
            It.IsAny<ReprocessorExporterAddOrganisation>(),
            It.IsAny<Person>(),
            It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task AddOrganisation_InvalidPartnerRole_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null; // Pass CHN validation
        organisationInput.Partners = [new() { Name = "Partner1", PartnerRole = "InvalidRole" }];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = new Dictionary<string, PartnerRole> { { "ValidRole", new PartnerRole { Id = 1, Name = "ValidRole" } } }.ToImmutableDictionary();
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = result as BadRequestObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        AssertValidationProblemDetails(objectResult.Value,
            "Partner role(s) does not exist",
            "PartnerRole",
            "Partner role 'InvalidRole' doesn't exist",
            "add-organisation/invalid-partner-role");
    }

    [TestMethod]
    public async Task AddOrganisation_InvalidPartnerRole_DoesNotCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null; // Pass CHN validation
        organisationInput.Partners = [new() { Name = "Partner1", PartnerRole = "InvalidRole" }];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = new Dictionary<string, PartnerRole> { { "ValidRole", new PartnerRole { Id = 1, Name = "ValidRole" } } }.ToImmutableDictionary();
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
            It.IsAny<ReprocessorExporterAddOrganisation>(),
            It.IsAny<Person>(),
            It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task AddOrganisation_InvitedUserAlreadyExists_ReturnsOkWithInviteeDetails()
    {
        // Arrange
        const string existingInvitedUserEmail = "existingUser@example.com";

        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        organisationInput.InvitedApprovedUsers =
        [
            new InvitedApprovedUserModel
            {
                JobTitle = "Director",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            }
        ];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        var mockResponse = new ReExAddOrganisationResponse
        {
            UserFirstName = "Test",
            UserLastName = "User",
            UserServiceRoles = new List<ServiceRoleResponse>
            {
                new ServiceRoleResponse
                {
                    Key = "test-key",
                    Name = "Test Role",
                    Description = "Test Description"
                }
            },
            OrganisationId = Guid.NewGuid(),
            ReferenceNumber = "REF123",
            InvitedApprovedUsers = new List<InvitedApprovedUserResponse>
            {
                new InvitedApprovedUserResponse
                {
                    Email = "invite@test.com",
                    InviteToken = "token123",
                    ServiceRole = new ServiceRoleResponse
                    {
                        Key = "approved-person",
                        Name = "Approved Person",
                        Description = "Approved Person Role"
                    }
                }
            }
        };

        _mockAccountService.Setup(s => s.AddReprocessorExporterOrganisationAsync(
                organisationInput, existingPerson, partnerRolesDict, ServiceKey, userId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        var objectResult = result.Should().BeOfType<OkObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = objectResult.Value.Should().BeOfType<ReExAddOrganisationResponse>().Subject;
        response.InvitedApprovedUsers.Should().BeEquivalentTo(mockResponse.InvitedApprovedUsers);
    }

    [TestMethod]
    public async Task AddOrganisation_InvitedUserAlreadyExists_DoesCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        const string existingInvitedUserEmail = "existingUser@example.com";

        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        organisationInput.InvitedApprovedUsers =
        [
            new InvitedApprovedUserModel
            {
                JobTitle = "Director",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            }
        ];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        var mockResponse = new ReExAddOrganisationResponse
        {
            UserFirstName = "Test",
            UserLastName = "User",
            UserServiceRoles = new List<ServiceRoleResponse>
            {
                new ServiceRoleResponse
                {
                    Key = "test-key",
                    Name = "Test Role",
                    Description = "Test Description"
                }
            },
            OrganisationId = Guid.NewGuid(),
            ReferenceNumber = "REF123",
            InvitedApprovedUsers = new List<InvitedApprovedUserResponse>
            {
                new InvitedApprovedUserResponse
                {
                    Email = "invite@test.com",
                    InviteToken = "token123",
                    ServiceRole = new ServiceRoleResponse
                    {
                        Key = "approved-person",
                        Name = "Approved Person",
                        Description = "Approved Person Role"
                    }
                }
            }
        };

        _mockAccountService.Setup(s => s.AddReprocessorExporterOrganisationAsync(
                organisationInput, existingPerson, partnerRolesDict, ServiceKey, userId))
            .ReturnsAsync(mockResponse);

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
                It.IsAny<ReprocessorExporterAddOrganisation>(),
                It.IsAny<Person>(),
                It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [TestMethod]
    public async Task AddOrganisation_InvitedUserInvitedTwice_ReturnsBadRequest()
    {
        // Arrange
        const string existingInvitedUserEmail = "existingUser@example.com";

        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        organisationInput.InvitedApprovedUsers =
        [
            new InvitedApprovedUserModel
            {
                JobTitle = "Director",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            },
            new InvitedApprovedUserModel
            {
                JobTitle = "Secretary",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            }
        ];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        var result = await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = result as BadRequestObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        AssertValidationProblemDetails(objectResult.Value,
            "Can't invite user multiple times",
            "InvitedApprovedUsers",
            "Can't invite user multiple times",
            "add-organisation/duplicate-invited-users");
    }

    [TestMethod]
    public async Task AddOrganisation_InvitedUserInvitedTwice_DoesNotCallAddReprocessorExporterOrganisationAsync()
    {
        // Arrange
        const string existingInvitedUserEmail = "existingUser@example.com";

        var userId = Guid.NewGuid();
        var organisationInput = CreateValidOrganisationInput(userId);
        organisationInput.Organisation.CompaniesHouseNumber = null;

        organisationInput.InvitedApprovedUsers =
        [
            new InvitedApprovedUserModel
            {
                JobTitle = "Director",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            },
            new InvitedApprovedUserModel
            {
                JobTitle = "Secretary",
                Person = new PersonModel
                {
                    ContactEmail = existingInvitedUserEmail, FirstName = "F", LastName = "L",
                    TelephoneNumber = "01234567890"
                }
            }
        ];

        var existingPerson = new Person { Id = 1, FirstName = "Test", LastName = "User" };
        _mockPersonService.Setup(s => s.GetPersonByUserId(userId)).ReturnsAsync(existingPerson);

        var partnerRolesDict = ImmutableDictionary<string, PartnerRole>.Empty;
        _mockPartnerService.Setup(s => s.GetPartnerRoles()).ReturnsAsync(partnerRolesDict);

        _mockOrganisationService.Setup(s => s.GetOrganisationsByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<OrganisationResponseModel>());

        // Act
        await _controller.AddOrganisation(organisationInput, ServiceKey, userId);

        // Assert
        _mockAccountService.Verify(s => s.AddReprocessorExporterOrganisationAsync(
                It.IsAny<ReprocessorExporterAddOrganisation>(),
                It.IsAny<Person>(),
                It.IsAny<IImmutableDictionary<string, PartnerRole>>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    private static void AssertValidationProblemDetails(
        object? val,
        string expectedDetail,
        string expectedErrorKey,
        string expectedErrorMessage,
        string expectedType)
    {
        var problemDetails = val.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Errors[expectedErrorKey].Length.Should().Be(1);
        string error = problemDetails.Errors[expectedErrorKey].First();
        error.Should().Be(expectedErrorMessage);
        problemDetails.Type.Should().Be(expectedType);
    }
}