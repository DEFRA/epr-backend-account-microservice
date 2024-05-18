using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Service = BackendAccountService.Data.Entities.Service;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class RoleManagementServiceTests
{
    private RoleManagementService _rmService;
    private AccountsDbContext _dbContext;
    private Mock<IValidationService> _mockValidationService;

    [TestInitialize]
    public void Setup()
    {
        _mockValidationService = new Mock<IValidationService>();
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(nameof(RegulatorServiceTests))
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new AccountsDbContext(contextOptions);
    }

    private void SetUpDatabase(Guid enrolmentExternalId, Guid userId, Guid organisationExternalId, string serviceKey, 
        bool addBasicUserEnrolment = false, bool addApprovedPersonEnrolment = false)
    {
        // Critical to avoid tests affecting one another, and previous runs holding old data
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
       .UseInMemoryDatabase("AccountsServiceTests")
       .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
       .Options;

        _dbContext = new AccountsDbContext(contextOptions);
        _rmService = new RoleManagementService(_dbContext, _mockValidationService.Object);

        _dbContext.Database.EnsureDeleted();

        var user = new User
        {
            UserId = userId,
            Id = 1,
            Email = "test1@test.com"
        };
        _dbContext.Users.Add(user);

        var person3 = new Person
        {
            User = user,
            FirstName = "Test",
            LastName = "User3",
            Email = "test3@abc.com",
            Telephone = "0123456789"
        };
        _dbContext.Persons.Add(person3);

        var organisation1 = new Organisation
        {
            Id = 1,
            Name = "Org 1",
            ExternalId = organisationExternalId,
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "111111"
        };
        _dbContext.Organisations.Add(organisation1);

        var personOrganisationConnection = new PersonOrganisationConnection
        {
            Organisation = organisation1,
            Person = person3,
            OrganisationRoleId = 0,
            PersonRoleId = 2,
            Id = 1,
        };

        _dbContext.PersonOrganisationConnections.Add(personOrganisationConnection);

        var personInOrganisationRoles = new List<PersonInOrganisationRole>
        {
            new PersonInOrganisationRole{ Id = 1, Name = "Admin"},
            new PersonInOrganisationRole{  Id = 2, Name = "Employee"}
        };

        _dbContext.PersonInOrganisationRoles.AddRange(personInOrganisationRoles);
               

        var service = new Service
        {
            Description = serviceKey,
            Key = serviceKey,
            Name = serviceKey
        };

        _dbContext.Services.Add(service);

        var approvedPersonServiceRole = new Data.Entities.ServiceRole
        {
            Key = "Packaging.ApprovedPerson",
            Description = "Packaging.ApprovedPerson",
            Name = "Packaging.ApprovedPerson",
            Service = service
        };

        if (enrolmentExternalId != Guid.Empty)
        {
            var enrolment = new Enrolment
            {
                ExternalId = enrolmentExternalId,
                ServiceRole = approvedPersonServiceRole,
                ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id,
                EnrolmentStatusId = 7,
                Connection = personOrganisationConnection,
                Id = 1                
            };
            _dbContext.Enrolments.Add(enrolment);
        }

        if (addBasicUserEnrolment)
        {
            var basicPersonServiceRole = new Data.Entities.ServiceRole
            {
                Key = "Packaging.BasicUser",
                Description = "Packaging.BasicUser",
                Name = "Packaging.BasicUser",
                Service = service
            };

            var enrolment2 = new Enrolment
            {
                ExternalId = enrolmentExternalId,
                ServiceRole = basicPersonServiceRole,
                EnrolmentStatusId = 7,
                Connection = personOrganisationConnection,
                Id = 2
            };
            _dbContext.Enrolments.Add(enrolment2);
        }

        if (addApprovedPersonEnrolment)
        {
            var approvedPersonEnrolment = new ApprovedPersonEnrolment
            {
                NomineeDeclaration = "First Surname",
                NomineeDeclarationTime = new DateTimeOffset(2024,1,1,11,41,00, new TimeSpan()),
                EnrolmentId = 1
            };
            _dbContext.Add(approvedPersonEnrolment);
        }
        
        _dbContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    [TestMethod]
    [DynamicData(nameof(AcceptNominationForApprovedPersonDataSource))]
    public async Task AcceptNominationForApprovedPerson(
        Guid externalId, Guid userId, Guid organisationExternalId, string serviceKey, bool expectedSuccess, string errorMessage)
    {
        // Arrange
        SetUpDatabase(externalId, userId, organisationExternalId, serviceKey);

        // Act
        var request = new AcceptNominationForApprovedPersonRequest { DeclarationFullName = "first last", DeclarationTimeStamp = DateTime.Now, JobTitle ="Professional", Telephone="01274889955" };
        var result = await _rmService.AcceptNominationForApprovedPerson(externalId, userId, organisationExternalId, serviceKey, request);

        // Assert
        result.Succeeded.Should().Be(expectedSuccess);
        result.ErrorMessage.Should().Be(errorMessage);
    }

    public static IEnumerable<object[]> AcceptNominationForApprovedPersonDataSource
    {
        get
        {
            return new[]
            {
                new object[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Packaging", true, string.Empty },
                new object[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Not packaging", false, "Unsupported service 'Not packaging'" },
                new object[] { Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), "Packaging", false, "There is no matching enrolment" },
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(AcceptNominationForApprovedPerson_RemovesBasicUserEnrolmentsDataSource))]
    public async Task AcceptNominationForApprovedPerson_RemovesBasicUserEnrolments(Guid externalId, Guid userId, Guid organisationExternalId, string serviceKey, bool expectedSuccess, string errorMessage)
    {
        // Arrange
        SetUpDatabase(externalId, userId, organisationExternalId, serviceKey, false, true);

        // Act
        var request = new AcceptNominationForApprovedPersonRequest { DeclarationFullName = "first last", DeclarationTimeStamp = DateTime.Now, JobTitle = "Professional", Telephone = "01274889955" };
        var result = await _rmService.AcceptNominationForApprovedPerson(externalId, userId, organisationExternalId, serviceKey, request);

        // Assert
        result.Succeeded.Should().Be(expectedSuccess);
        result.ErrorMessage.Should().Be(errorMessage);
        _dbContext.PersonOrganisationConnections.Where(x => x.Id == 1).FirstOrDefault().Enrolments.Count.Should().Be(1); 
    }

    public static IEnumerable<object[]> AcceptNominationForApprovedPerson_RemovesBasicUserEnrolmentsDataSource
    { 
        get
        {
            return new[]
            {
                new object[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Packaging", true, string.Empty },
            };
        }    
    }

    [TestMethod]
    [DynamicData(nameof(AcceptNominationForApprovedPerson_ExistingApprovedPersonEnrolmentUpdatedDataSource))]
    public async Task AcceptNominationForApprovedPerson_ExistingApprovedPersonEnrolmentUpdated(Guid externalId, Guid userId, 
        Guid organisationExternalId, string serviceKey, bool expectedSuccess, string errorMessage)
    {
        // Arrange
        SetUpDatabase(externalId, userId, organisationExternalId, serviceKey, true);

        // Act
        var request = new AcceptNominationForApprovedPersonRequest { DeclarationFullName = "first last", DeclarationTimeStamp = DateTime.Now, JobTitle = "Professional", Telephone = "01274889955" };
        var result = await _rmService.AcceptNominationForApprovedPerson(externalId, userId, organisationExternalId, serviceKey, request);

        // Assert
        result.Succeeded.Should().Be(expectedSuccess);
        result.ErrorMessage.Should().Be(errorMessage);
        _dbContext.ApprovedPersonEnrolments.Where(x => x.Id == 1).FirstOrDefault().NomineeDeclaration.Should().Be(request.DeclarationFullName);
        _dbContext.ApprovedPersonEnrolments.Where(x => x.Id == 1).FirstOrDefault().NomineeDeclarationTime.Should().Be(request.DeclarationTimeStamp);
    }

    [TestMethod]
    [DynamicData(nameof(AcceptNominationForApprovedPerson_ExistingApprovedPersonEnrolmentUpdatedDataSource))]
    public async Task AcceptNominationForApprovedPerson_PersonInOrganizationRoleIsSetToAdmin(Guid externalId, Guid userId,
        Guid organisationExternalId, string serviceKey, bool expectedSuccess, string errorMessage)
    {
        // Arrange
        SetUpDatabase(externalId, userId, organisationExternalId, serviceKey, true);

        // Act
        var request = new AcceptNominationForApprovedPersonRequest { DeclarationFullName = "first last", DeclarationTimeStamp = DateTime.Now, JobTitle = "Professional", Telephone = "01274889955" };
        var result = await _rmService.AcceptNominationForApprovedPerson(externalId, userId, organisationExternalId, serviceKey, request);

        // Assert
        result.Succeeded.Should().Be(expectedSuccess);
        result.ErrorMessage.Should().Be(errorMessage);
        _dbContext.PersonOrganisationConnections.FirstOrDefault().PersonRole.Name.Should().Be("Admin");
       
    }

    public static IEnumerable<object[]> AcceptNominationForApprovedPerson_ExistingApprovedPersonEnrolmentUpdatedDataSource
    {
        get
        {
            return new[]
            {
                new object[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Packaging", true, string.Empty },
            };
        }
    }
}