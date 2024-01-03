using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class ConnectionsServiceTests
{
    private AccountsDbContext _accountContext= null!;
    private RoleManagementService _connectionsService = null!;
    private static DbContextOptions<AccountsDbContext> _contextOptions = default;
    private static readonly Guid userId = new Guid("00000000-0000-0000-0000-000000000001");
    
    private readonly string validServiceKey = "Packaging";
    private static readonly Guid validConnectionId = new Guid("00000000-0000-0000-0000-000000000010");
    private static readonly Guid validOrganisationId = new Guid("00000000-0000-0000-0000-000000000020");
    private static readonly Guid validEnrolmentId = new Guid("00000000-0000-0000-0000-000000000030");
    private static readonly Guid validUserId = new Guid("00000000-0000-0000-0000-000000000040");
    private static readonly int validNominatorEnrolmentId = 1;

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
        _accountContext = new AccountsDbContext(_contextOptions);
        _connectionsService = new RoleManagementService(_accountContext, new ValidationService(_accountContext, NullLogger<ValidationService>.Instance));
    }

    [TestMethod]
    public async Task GetConnectionWithPersonForServiceAsync_WhenValidRequest_ThenReturnUser()
    {
        var result = await _connectionsService.GetConnectionWithPersonForServiceAsync(validConnectionId, validOrganisationId, validServiceKey);
        result.Should().BeOfType<ConnectionWithPersonResponse>();
        result.FirstName.Should().Be("Testy");
        result.LastName.Should().Be("McTest");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    [DataRow(Data.Entities.Conversions.RelationshipType.Employment)]
    [DataRow(Data.Entities.Conversions.RelationshipType.Consultancy)]
    [DataRow(Data.Entities.Conversions.RelationshipType.ComplianceScheme)]
    [DataRow(Data.Entities.Conversions.RelationshipType.Other)]
    [DataRow(0)]
    public async Task NominateToDelegatedPerson_WhenNominationRequestIsIncomplete_ThenNominationFails(Data.Entities.Conversions.RelationshipType relationshipType)
    {
        var result = await _connectionsService.NominateToDelegatedPerson(validConnectionId, userId, validOrganisationId, validServiceKey, new Models.Request.DelegatedPersonNominationRequest
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
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = Data.Entities.Conversions.RelationshipType.Employment,
            JobTitle = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Employment relationship requires Job Title to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingConsultancyName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = Data.Entities.Conversions.RelationshipType.Consultancy,
            ConsultancyName = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Consultancy relationship requires Consultancy Name to be specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingComplianceSchemeName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = Data.Entities.Conversions.RelationshipType.ComplianceScheme,
            ComplianceSchemeName = string.Empty
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Compliance Scheme relationship requires Compliance Scheme name specified");
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public void ValidateNominationRequest_WhenNominationRequestIsMissingOtherOrganisationName_ThenItsValidationFails()
    {
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = Data.Entities.Conversions.RelationshipType.Other,
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
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = Data.Entities.Conversions.RelationshipType.Other,
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
        var validationResult = RoleManagementService.ValidateDelegatedPersonNominationRequest(new Models.Request.DelegatedPersonNominationRequest
        {
            RelationshipType = default
        });

        validationResult.Succeeded.Should().BeFalse();
        validationResult.ErrorMessage.Should().Be("Unsupported relationship type");
    }
    
    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsValid_ReturnsValidResponse()
    {
        var result = await _connectionsService.GetDelegatedPersonNominator(validEnrolmentId, validUserId, validOrganisationId, validServiceKey);
        result.Should().BeOfType<DelegatedPersonNominatorResponse>();
        result.FirstName.Should().Be("Testy");
        result.LastName.Should().Be("McTest");
        result.OrganisationName.Should().Be("Test org 1");
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        setupContext.Database.EnsureCreated();

        var service = new Data.Entities.Service
        {
            Description = "Packaging",
            Key = "Packaging",
            Name = "Packaging"
        };
        setupContext.Services.Add(service);
        
        var approvedPersonServiceRole = new Data.Entities.ServiceRole
        {
            Key = "Packaging.ApprovedPerson",
            Description = "Packaging.ApprovedPerson",
            Name = "Packaging.ApprovedPerson",
            Service = service
        };
        var delegatedPersonServiceRole = new Data.Entities.ServiceRole
        {
            Key = "Packaging.DelegatedPerson",
            Description = "Packaging.DelegatedPerson",
            Name = "Packaging.DelegatedPerson",
            Service = service
        };
        setupContext.ServiceRoles.Add(approvedPersonServiceRole);

        var approvedPersonEnrolment = new Enrolment
        {
            Id = validNominatorEnrolmentId,
            ExternalId = Guid.NewGuid(),
            ServiceRole = approvedPersonServiceRole,
            EnrolmentStatusId = EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = validConnectionId,
                JobTitle = "test",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = "Testy",
                    LastName = "McTest",
                    Email = "test@test.com",
                    Telephone = "07907867564",
                    User = new User
                    {
                        UserId = userId,
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
                    IsComplianceScheme = false,
                    ValidatedWithCompaniesHouse = true,
                    Name = "Test org 1",
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
                    ExternalId = validOrganisationId
                }
            }
        };
        
        var delegatedUserEnrolment = new DelegatedPersonEnrolment()
        {
            Id = 1,
            Enrolment = new Enrolment()
            {
                Id = 2,
                ExternalId = validEnrolmentId,
                ServiceRole = delegatedPersonServiceRole,
                EnrolmentStatusId = EnrolmentStatus.Nominated,
                Connection = new PersonOrganisationConnection
                {
                    Organisation = new Organisation
                    {
                        ExternalId = validOrganisationId,
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
                            UserId = validUserId
                        }
                    }
                }
            },
            NominatorEnrolment = approvedPersonEnrolment
        };
        
        setupContext.Enrolments.Add(approvedPersonEnrolment);
        setupContext.DelegatedPersonEnrolments.Add(delegatedUserEnrolment);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}
