using BackendAccountService.Core.Models.Exceptions;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using Service = BackendAccountService.Data.Entities.Service;
using ServiceRole = BackendAccountService.Data.Entities.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class ConnectionsServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private RoleManagementService _connectionsService = null!;
    private static DbContextOptions<AccountsDbContext>? _contextOptions;

    private const int ValidNominatorEnrolmentId = 1;
    private const string ValidServiceKey = "Packaging";
    private const string PersonFirstName = "TestFirstName";
    private const string PersonLastName = "TestLastName";
    private const string PersonEmail = "test@test.com";
    private const string OrganisationName = "TestOrganisationName";
    private const string OrganisationReferenceNumber = "TestOrganisationReferenceNumber";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ApprovedUserConnectionId = Guid.NewGuid();
    private static readonly Guid DelegatedUserConnectionId = Guid.NewGuid();
    private static readonly Guid DelegatedUserConnectionId2 = Guid.NewGuid();
    private static readonly Guid UnknownUserConnectionId = Guid.NewGuid();
    private static readonly Guid NoConnectionId = Guid.NewGuid();
    private static readonly Guid InvitedEnrolmentConnectionId = Guid.NewGuid();
    private static readonly Guid ValidOrganisationId = Guid.NewGuid();
    private static readonly Guid ValidEnrolmentId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        _contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("ConnectionsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(_contextOptions);
    }

    [TestInitialize]
    public void Setup()
    {
        _accountContext = new AccountsDbContext(_contextOptions!);
        _connectionsService = new RoleManagementService(_accountContext, new ValidationService(_accountContext, NullLogger<ValidationService>.Instance));
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    [DataRow(RelationshipType.Employment)]
    [DataRow(RelationshipType.Consultancy)]
    [DataRow(RelationshipType.ComplianceScheme)]
    [DataRow(RelationshipType.Other)]
    [DataRow(0)]
    public async Task NominateToDelegatedPerson_WhenNominationRequestIsIncomplete_ThenNominationFails(RelationshipType relationshipType)
    {
        var result = await _connectionsService.NominateToDelegatedPerson(ApprovedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, new DelegatedPersonNominationRequest
        {
            RelationshipType = relationshipType
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingJobTitle_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Employment relationship requires Job Title to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingConsultancyName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Consultancy,
            ConsultancyName = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Consultancy relationship requires Consultancy Name to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingComplianceSchemeName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Compliance Scheme relationship requires Compliance Scheme name specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingOtherOrganisationName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Other,
            OtherOrganisationName = string.Empty,
            OtherRelationshipDescription = "Other Relationship Description"
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Other Organisation relationship requires Organisation Name to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingOtherRelationshipDescription_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Other,
            OtherOrganisationName = "Other Organisation Name",
            OtherRelationshipDescription = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Other Organisation relationship requires Relationship Description to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingRelationshipType_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new DelegatedPersonNominationRequest
        {
            RelationshipType = default
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Unsupported relationship type");
    }

    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsValid_ReturnsValidResponse()
    {
        var result = await _connectionsService.GetDelegatedPersonNominator(ValidEnrolmentId, ValidUserId, ValidOrganisationId, ValidServiceKey);
        result.Should().BeOfType<DelegatedPersonNominatorResponse>();
        result.FirstName.Should().Be(PersonFirstName);
        result.LastName.Should().Be(PersonLastName);
        result.OrganisationName.Should().Be(OrganisationName);
    }

    [TestMethod]
    public async Task GetConnectionWithEnrolmentsFromOrganisationForServiceAsync_ReturnsListWithEnrolments()
    {
        // Act
        var result = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(ApprovedUserConnectionId, ValidOrganisationId, ValidServiceKey);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ConnectionWithEnrolmentsResponse>();
        result?.Enrolments.Should().HaveCount(1);
        result?.PersonRole.Should().Be(Models.PersonRole.Admin);
    }

    [TestMethod]
    public async Task GetConnectionWithPersonForServiceAsync_ReturnsListWithPerson()
    {
        // Act
        var result = await _connectionsService.GetConnectionWithPersonForServiceAsync(ApprovedUserConnectionId, ValidOrganisationId, ValidServiceKey);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ConnectionWithPersonResponse>();
        result?.OrganisationName.Should().Be(OrganisationName);
        result?.OrganisationReferenceNumber.Should().Be(OrganisationReferenceNumber);
        result?.Email.Should().Be(PersonEmail);
        result?.FirstName.Should().Be(PersonFirstName);
        result?.LastName.Should().Be(PersonLastName);
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenServiceKeyNotPackaging_ThrowsRoleManagementException()
    {
        // Arrange
        const string invalidServiceKey = "SomethingOtherThanPackaging";

        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(ApprovedUserConnectionId, UserId, ValidOrganisationId, invalidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenPersonRoleNotEmployeeNorAdmin_ThrowsRoleManagementException()
    {
        // Arrange
        const int invalidPersonRole = PersonRole.NotSet;

        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(ApprovedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, invalidPersonRole));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenConnectionDoesNotExist_ThrowsRoleManagementException()
    {
        // Arrange
        var invalidConnectionId = Guid.NewGuid();

        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(invalidConnectionId, UserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenConnectionPersonUserAndUserAreSame_ThrowsRoleManagementException()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(ApprovedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenNoActiveEnrolment_ThrowsRoleManagementException()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(NoConnectionId, UserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenActiveEnrolmentInInvitedStatus_ThrowsRoleManagementException()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(InvitedEnrolmentConnectionId, UserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenEnrolmentPersonIsApproved_ThrowsRoleManagementException()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<RoleManagementException>(() =>
             _connectionsService.UpdatePersonRoleAsync(ApprovedUserConnectionId, ValidUserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin));
    }

    [TestMethod]
    [TestCategory("UpdatePersonRoleAsync")]
    public async Task UpdatePersonRoleAsync_WhenEnrolmentPersonIsDelegated_ThrowsRoleManagementException()
    {
        // Act
        var result = await _connectionsService.UpdatePersonRoleAsync(DelegatedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, Models.PersonRole.Admin);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UpdatePersonRoleResponse>();
        result.RemovedServiceRoles.Should().HaveCount(1);
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenServiceKeyNotPackaging_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        const string invalidServiceKey = "SomethingOtherThanPackaging";

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(ApprovedUserConnectionId, UserId, ValidOrganisationId, invalidServiceKey, new DelegatedPersonNominationRequest());

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be($"Unsupported service '{invalidServiceKey}'");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenInvalidRequest_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = default
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(ApprovedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid nomination request: Unsupported relationship type");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenConnectionDoesNotExist_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var invalidConnectionId = Guid.NewGuid();
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(invalidConnectionId, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("There is no matching record to update");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenConnectionPersonUserAndUserAreSame_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(ApprovedUserConnectionId, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Updating own record is not permitted");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenNoActiveEnrolment_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(NoConnectionId, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Not enrolled user cannot be nominated");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenActiveEnrolmentInInvitedStatus_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(InvitedEnrolmentConnectionId, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invited user cannot be nominated");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenEnrolmentPersonIsApproved_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(ApprovedUserConnectionId, ValidUserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Approved Person cannot be nominated");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenEnrolmentPersonIsDelegated_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(DelegatedUserConnectionId2, UserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Delegated Person cannot be nominated");
    }

    [TestMethod]
    [TestCategory("NominateToDelegatedPerson")]
    public async Task NominateToDelegatedPerson_WhenEnrolmentPersonIsUnknown_ReturnsUnsuccessfulResponse()
    {
        // Arrange
        var request = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.Employment,
            JobTitle = "DummyJobTitle"
        };

        // Act
        var result = await _connectionsService.NominateToDelegatedPerson(UnknownUserConnectionId, ValidUserId, ValidOrganisationId, ValidServiceKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Only Basic User can be nominated");
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        setupContext.Database.EnsureCreated();

        var service = new Service
        {
            Description = "Packaging",
            Key = "Packaging",
            Name = "Packaging"
        };
        setupContext.Services.Add(service);

        var approvedPersonServiceRole = new ServiceRole
        {
            Key = "Packaging.ApprovedPerson",
            Description = "Packaging.ApprovedPerson",
            Name = "Packaging.ApprovedPerson",
            Service = service
        };
        var delegatedPersonServiceRole = new ServiceRole
        {
            Key = "Packaging.DelegatedPerson",
            Description = "Packaging.DelegatedPerson",
            Name = "Packaging.DelegatedPerson",
            Service = service
        };

        var unknownServiceRole = new ServiceRole
        {
            Key = "Packaging.Unknown",
            Description = "Packaging.Unknown",
            Name = "Packaging.Unknown",
            Service = service
        };

        setupContext.ServiceRoles.Add(approvedPersonServiceRole);

        var approvedPersonEnrolment = new Enrolment
        {
            Id = ValidNominatorEnrolmentId,
            ExternalId = Guid.NewGuid(),
            ServiceRole = approvedPersonServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = ApprovedUserConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = UserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var unknownPersonEnrolment = new Enrolment
        {
            Id = 800,
            ExternalId = Guid.NewGuid(),
            ServiceRole = unknownServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = UnknownUserConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = UserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var personEnrolmentWithInactiveEnrolment = new Enrolment
        {
            Id = 666,
            ExternalId = Guid.NewGuid(),
            ServiceRole = new ServiceRole { Key = "SomeKey", Name = "SomeName2", Service = new Service { Key = "InvalidKey", Name = "SomeName" } },
            EnrolmentStatusId = EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = NoConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = ValidUserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var invitedEnrolment = new Enrolment
        {
            Id = 667,
            ExternalId = Guid.NewGuid(),
            ServiceRole = approvedPersonServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Invited,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = InvitedEnrolmentConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = ValidUserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var delegatedEnrolment = new Enrolment
        {
            Id = 669,
            ExternalId = Guid.NewGuid(),
            ServiceRole = delegatedPersonServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Pending,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = DelegatedUserConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = ValidUserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var delegatedEnrolment2 = new Enrolment
        {
            Id = 601,
            ExternalId = Guid.NewGuid(),
            ServiceRole = delegatedPersonServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Pending,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = DelegatedUserConnectionId2,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = PersonFirstName,
                    LastName = PersonLastName,
                    Email = PersonEmail,
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = ValidUserId,
                        ExternalIdpId = "test",
                        ExternalIdpUserId = "test",
                        Email = "test@test.com"
                    }
                },
                Organisation = new Organisation
                {
                    OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                    ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
                    CompaniesHouseNumber = "123456",
                    ReferenceNumber = OrganisationReferenceNumber,
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = OrganisationName,
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
                    NationId = Data.DbConstants.Nation.England,
                    ExternalId = ValidOrganisationId
                }
            }
        };

        var delegatedUserEnrolment = new DelegatedPersonEnrolment()
        {
            Id = 1,
            Enrolment = new Enrolment()
            {
                Id = 2,
                ExternalId = ValidEnrolmentId,
                ServiceRole = delegatedPersonServiceRole,
                EnrolmentStatusId = EnrolmentStatus.Nominated,
                Connection = new PersonOrganisationConnection
                {
                    Organisation = new Organisation
                    {
                        ExternalId = ValidOrganisationId,
                        Name = "Test Org Name"
                    },
                    PersonRoleId = PersonRole.Admin,
                    Person = new Person
                    {
                        FirstName = "Delegated",
                        LastName = "User",
                        Email = "Delegated@test.com",
                        Telephone = "07907867564",
                        User = new User
                        {
                            UserId = ValidUserId
                        }
                    }
                }
            },
            NominatorEnrolment = approvedPersonEnrolment
        };

        setupContext.Enrolments.Add(approvedPersonEnrolment);
        setupContext.Enrolments.Add(personEnrolmentWithInactiveEnrolment);
        setupContext.Enrolments.Add(invitedEnrolment);
        setupContext.Enrolments.Add(delegatedEnrolment);
        setupContext.Enrolments.Add(delegatedEnrolment2);
        setupContext.Enrolments.Add(unknownPersonEnrolment);
        setupContext.DelegatedPersonEnrolments.Add(delegatedUserEnrolment);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}