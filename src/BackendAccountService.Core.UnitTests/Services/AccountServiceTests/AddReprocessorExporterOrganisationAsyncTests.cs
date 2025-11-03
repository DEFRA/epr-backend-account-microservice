using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System.Collections.Immutable;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRole = BackendAccountService.Data.DbConstants.PersonRole;
using ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services.AccountServiceTests;

[TestClass]
public class AddReprocessorExporterOrganisationAsyncTests
{
    const string ServiceKey = "ServiceKey";

    private AccountsDbContext _dbContext;
    private Mock<ITokenService> _mockTokenService;
    private Mock<IReExEnrolmentMaps> _mockReExEnrolmentMaps;
    private AccountService _accountService;

    [TestInitialize]
    public async Task TestInitialize()
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(databaseName: $"Test_Db_InMemory_{Guid.NewGuid()}")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AccountsDbContext(options);

        await _dbContext.ServiceRoles.AddRangeAsync(
            new Data.Entities.ServiceRole
            {
                Id = ServiceRole.ReprocessorExporter.AdminUser.Id,
                Name = "Admin",
                Key = ServiceRole.ReprocessorExporter.AdminUser.Key,
                Description = "Admin description"
            },
            new Data.Entities.ServiceRole
            {
                Id = ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                Name = "Approved",
                Key = ServiceRole.ReprocessorExporter.ApprovedPerson.Key,
                Description = "Approved description"
            });

        _mockTokenService = new Mock<ITokenService>();
        _mockReExEnrolmentMaps = new Mock<IReExEnrolmentMaps>();

        _accountService = new AccountService(
            _dbContext,
            _mockTokenService.Object,
            _mockReExEnrolmentMaps.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _dbContext.Dispose();
    }

    // --- Test Data Helper Methods ---
    private static ReprocessorExporterAddOrganisation CreateValidReprocAddOrgInput(
        Guid actorUserGuid,
        bool isApprovedUser = true)
    {
        return new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel
            {
                UserId = actorUserGuid,
                IsApprovedUser = isApprovedUser,
                JobTitle = "Director"
            },
            Organisation = new ReprocessorExporterOrganisationModel
            {
                Name = "InMem Test Org",
                Address = new AddressModel { Town = "InMemoryVille" },
                OrganisationType = Models.OrganisationType.CompaniesHouseCompany,
                Nation = Models.Nation.Wales,
                ProducerType = Models.ProducerType.Other
            },
            Partners = [],
            InvitedApprovedUsers = [],
            DeclarationTimeStamp = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    private async Task<Person> CreatePersonAndAddToContext(Guid actorUserGuid)
    {
        const int personDbId = 1, userDbId = 1;

        var userEntity = new User
        {
            Id = userDbId,          // User's database primary key (int)
            UserId = actorUserGuid  // User's Guid identifier
        };

        var person = new Person
        {
            Id = personDbId,        // Person's own database primary key (int)
            UserId = userDbId,      // Foreign key to User.Id (int)
            User = userEntity,      // Navigation property to the User
            Email = $"{Guid.NewGuid()}@example.com",
            FirstName = "InMem",
            LastName = "Actor",
            Telephone = "01234567890"
        };

        await _dbContext.Persons.AddAsync(person);
        await _dbContext.SaveChangesAsync(null, null, null);

        return person;
    }

    private static Enrolment SetupMockMainEnrolmentForMapping(
        ReprocessorExporterAddOrganisation account,
        Person person)
    {
        return new Enrolment
        {
            Connection = new PersonOrganisationConnection
            {
                Person = person,
                Organisation = new Organisation
                {
                    Name = account.Organisation.Name,
                    ExternalId = Guid.NewGuid(),
                    OrganisationTypeId = (int)account.Organisation.OrganisationType,
                    NationId = (int?)account.Organisation.Nation
                },
                JobTitle = account.User.JobTitle,
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            ServiceRoleId = ServiceRole.ReprocessorExporter.AdminUser.Id,
            EnrolmentStatusId = EnrolmentStatus.Enrolled
        };
    }

    private static Enrolment SetupMockApprovedEnrolmentForMapping(
        ReprocessorExporterAddOrganisation account,
        Person person,
        PersonOrganisationConnection connection)
    {
        var mappedEnrolment = new Enrolment
        {
            Connection = connection,
            ServiceRoleId = ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
            EnrolmentStatusId = EnrolmentStatus.Enrolled,
            ApprovedPersonEnrolment = new ApprovedPersonEnrolment
            {
                NomineeDeclaration = $"{person.FirstName} {person.LastName}",
                NomineeDeclarationTime = (DateTimeOffset)account.DeclarationTimeStamp
            }
        };
        return mappedEnrolment;
    }

    private static Enrolment SetupMockInvitedEnrolmentForMapping(
        InvitedApprovedUserModel invitedUserModel,
        Organisation targetOrganisationFromMainEnrolment,
        string inviterEmail,
        string generatedToken)
    {
        var invitedPersonEntity = new Person
        {
            FirstName = invitedUserModel.Person.FirstName,
            LastName = invitedUserModel.Person.LastName,
            Email = invitedUserModel.Person.ContactEmail,
            Telephone = invitedUserModel.Person.TelephoneNumber,
            User = new User
            {
                UserId = Guid.Empty,
                Email = invitedUserModel.Person.ContactEmail,
                InviteToken = generatedToken,
                InvitedBy = inviterEmail
            }
        };
        var mappedConnection = new PersonOrganisationConnection
        {
            JobTitle = invitedUserModel.JobTitle,
            Organisation = targetOrganisationFromMainEnrolment,
            Person = invitedPersonEntity,
            PersonRoleId = PersonRole.Employee
        };
        return new Enrolment
        {
            Connection = mappedConnection,
            ServiceRoleId = ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
            EnrolmentStatusId = EnrolmentStatus.Invited
        };
    }

    [TestMethod]
    [DataRow(true, DisplayName = "ApprovedUser")]
    [DataRow(false, DisplayName = "NonApprovedUser")]
    public async Task AddReprocessorExporterOrganisationAsync_NoPartnersOrInvitedUsers_SavesAdminEnrolmentToDb(bool isApprovedUser)
    {
        // Arrange
        var actorUserGuid = Guid.NewGuid(); // This is User.UserId (Guid)
        var accountInput = CreateValidReprocAddOrgInput(actorUserGuid, isApprovedUser);
        // Create a Person entity consistent with actorUserGuid
        var personInput = await CreatePersonAndAddToContext(actorUserGuid);

        var partnerRolesInput = ImmutableDictionary<string, PartnerRole>.Empty;

        var mainEnrolmentToBeMappedAndAdded = SetupMockMainEnrolmentForMapping(accountInput, personInput);
        _mockReExEnrolmentMaps.Setup(m => m.GetAdminEnrolmentForCurrentUser(accountInput, personInput))
            .Returns(mainEnrolmentToBeMappedAndAdded);

        _mockReExEnrolmentMaps.Setup(m =>
                m.GetApprovedPersonEnrolmentForCurrentUser(accountInput, personInput,
                    It.IsAny<PersonOrganisationConnection>()))
            .Returns(new Enrolment());

        // Act
        await _accountService.AddReprocessorExporterOrganisationAsync(
            accountInput, personInput, partnerRolesInput, ServiceKey, actorUserGuid);

        // Assert
        var savedEnrolment = await _dbContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection.Organisation)
            .Include(e => e.Connection.Person).ThenInclude(p => p.User) // Include User for verification
            .Include(e => e.ApprovedPersonEnrolment)
            .SingleOrDefaultAsync(e => e.Connection.Organisation.ExternalId == mainEnrolmentToBeMappedAndAdded.Connection.Organisation.ExternalId
                                                && e.ServiceRoleId == ServiceRole.ReprocessorExporter.AdminUser.Id, default);

        savedEnrolment.Should().NotBeNull();
        savedEnrolment.Connection.Organisation.Name.Should().Be(accountInput.Organisation.Name);
        savedEnrolment.Connection.Person.Id.Should().Be(personInput.Id); // Check if the correct Person entity is linked
        savedEnrolment.Connection.Person.User.UserId.Should().Be(actorUserGuid); // Verify linked User's Guid
        savedEnrolment.ServiceRoleId.Should().Be(mainEnrolmentToBeMappedAndAdded.ServiceRoleId);
        savedEnrolment.EnrolmentStatusId.Should().Be(EnrolmentStatus.Enrolled);
    }

    [TestMethod]
    public async Task AddReprocessorExporterOrganisationAsync_ApprovedUserNoPartnersOrInvitedUsers_SavesApprovedEnrolmentToDb()
    {
        // Arrange
        var actorUserGuid = Guid.NewGuid(); // This is User.UserId (Guid)
        var accountInput = CreateValidReprocAddOrgInput(actorUserGuid, isApprovedUser: true);
        // Create a Person entity consistent with actorUserGuid
        var personInput = await CreatePersonAndAddToContext(actorUserGuid);

        var partnerRolesInput = ImmutableDictionary<string, PartnerRole>.Empty;

        var mainEnrolmentToBeMappedAndAdded = SetupMockMainEnrolmentForMapping(accountInput, personInput);
        _mockReExEnrolmentMaps.Setup(m => m.GetAdminEnrolmentForCurrentUser(accountInput, personInput))
            .Returns(mainEnrolmentToBeMappedAndAdded);

        var approvedUserEnrolment = SetupMockApprovedEnrolmentForMapping(accountInput, personInput, mainEnrolmentToBeMappedAndAdded.Connection);
        _mockReExEnrolmentMaps.Setup(m =>
                m.GetApprovedPersonEnrolmentForCurrentUser(accountInput, personInput,
                    It.IsAny<PersonOrganisationConnection>()))
                .Returns(approvedUserEnrolment);

        // Act
        await _accountService.AddReprocessorExporterOrganisationAsync(
            accountInput, personInput, partnerRolesInput, ServiceKey, actorUserGuid);

        // Assert
        var savedEnrolment = await _dbContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection.Organisation)
            .Include(e => e.Connection.Person).ThenInclude(p => p.User) // Include User for verification
            .Include(e => e.ApprovedPersonEnrolment)
            .SingleOrDefaultAsync(e => e.Connection.Organisation.ExternalId == mainEnrolmentToBeMappedAndAdded.Connection.Organisation.ExternalId
                                                && e.ServiceRoleId == ServiceRole.ReprocessorExporter.ApprovedPerson.Id, default);

        savedEnrolment.Should().NotBeNull();
        savedEnrolment.Connection.Organisation.Name.Should().Be(accountInput.Organisation.Name);
        savedEnrolment.Connection.Person.Id.Should().Be(personInput.Id); // Check if the correct Person entity is linked
        savedEnrolment.Connection.Person.User.UserId.Should().Be(actorUserGuid); // Verify linked User's Guid
        savedEnrolment.EnrolmentStatusId.Should().Be(EnrolmentStatus.Enrolled);

        savedEnrolment.ApprovedPersonEnrolment.Should().NotBeNull();
        savedEnrolment.ApprovedPersonEnrolment.NomineeDeclaration.Should().Be($"{personInput.FirstName} {personInput.LastName}");
        savedEnrolment.ApprovedPersonEnrolment.NomineeDeclarationTime.Should().BeCloseTo((DateTimeOffset)accountInput.DeclarationTimeStamp, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task AddReprocessorExporterOrganisationAsync_NoPartnersOrInvitedUsers_ReturnsCorrectResult(bool isApprovedUser)
    {
        // Arrange
        var actorUserGuid = Guid.NewGuid(); // This is User.UserId (Guid)
        var accountInput = CreateValidReprocAddOrgInput(actorUserGuid, isApprovedUser);
        // Create a Person entity consistent with actorUserGuid
        var personInput = await CreatePersonAndAddToContext(actorUserGuid);

        var partnerRolesInput = ImmutableDictionary<string, PartnerRole>.Empty;

        var mainEnrolmentToBeMappedAndAdded = SetupMockMainEnrolmentForMapping(accountInput, personInput);
        _mockReExEnrolmentMaps.Setup(m => m.GetAdminEnrolmentForCurrentUser(accountInput, personInput))
            .Returns(mainEnrolmentToBeMappedAndAdded);

        _mockReExEnrolmentMaps.Setup(m =>
                m.GetApprovedPersonEnrolmentForCurrentUser(accountInput, personInput,
                    It.IsAny<PersonOrganisationConnection>()))
            .Returns(new Enrolment());

        var expectedFirstName = personInput.FirstName;
        var expectedLastName = personInput.LastName;
        var expectedReferenceNumber = "";

        // Act
        var result = await _accountService.AddReprocessorExporterOrganisationAsync(
            accountInput, personInput, partnerRolesInput, ServiceKey, actorUserGuid);

        // Assert
        result.UserFirstName.Should().Be(expectedFirstName);
        result.UserLastName.Should().Be(expectedLastName);
        result.OrganisationId.Should().Be(mainEnrolmentToBeMappedAndAdded.Connection.Organisation.ExternalId);
        result.ReferenceNumber.Should().Be(mainEnrolmentToBeMappedAndAdded.Connection.Organisation.ReferenceNumber);
        result.InvitedApprovedUsers.Should().BeEmpty();

        result.UserServiceRoles.Should().Contain(r => r.Key == ServiceRole.ReprocessorExporter.AdminUser.Key);
        if (isApprovedUser)
        {
            result.UserServiceRoles.Should().Contain(r => r.Key == ServiceRole.ReprocessorExporter.ApprovedPerson.Key);
        }
        else
        {
            result.UserServiceRoles.Should().NotContain(r => r.Key == ServiceRole.ReprocessorExporter.ApprovedPerson.Key);
        }
    }

    [TestMethod]
    public async Task AddReprocessorExporterOrganisationAsync_WithPartners_SavesPartnersToDb()
    {
        // Arrange
        var actorUserGuid = Guid.NewGuid();
        var accountInput = CreateValidReprocAddOrgInput(actorUserGuid);
        accountInput.Partners.Add(new PartnerModel { Name = "Partner One Inc.", PartnerRole = "PrimaryPartner" });
        var personInput = await CreatePersonAndAddToContext(actorUserGuid);
        var partnerRolesInput = new Dictionary<string, PartnerRole>
            { { "PrimaryPartner", new PartnerRole { Id = 11, Name = "PrimaryPartner" } } }.ToImmutableDictionary();

        var mainEnrolmentMapped = SetupMockMainEnrolmentForMapping(accountInput, personInput);
        _mockReExEnrolmentMaps.Setup(m => m.GetAdminEnrolmentForCurrentUser(accountInput, personInput))
            .Returns(mainEnrolmentMapped);

        _mockReExEnrolmentMaps.Setup(m =>
                m.GetApprovedPersonEnrolmentForCurrentUser(accountInput, personInput,
                    It.IsAny<PersonOrganisationConnection>()))
            .Returns(new Enrolment());

        // Act
        await _accountService.AddReprocessorExporterOrganisationAsync(
            accountInput, personInput, partnerRolesInput, ServiceKey, actorUserGuid);

        // Assert
        var savedMainOrganisation = await _dbContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ExternalId == mainEnrolmentMapped.Connection.Organisation.ExternalId, default);
        savedMainOrganisation.Should().NotBeNull();

        var savedPartnerLinks = await _dbContext.OrganisationToPartnerRoles
            .AsNoTracking()
            .Where(otp => otp.OrganisationId == savedMainOrganisation.Id)
            .ToListAsync(default);
        savedPartnerLinks.Should().HaveCount(1);
        savedPartnerLinks.Should().Contain(p => p.Name == "Partner One Inc." && p.PartnerRoleId == 11);
    }

    [TestMethod]
    public async Task AddReprocessorExporterOrganisationAsync_WithInvitedUsers_SavesInvitedUserEnrolmentsToDb()
    {
        // Arrange
        var actorUserGuid = Guid.NewGuid();
        var accountInput = CreateValidReprocAddOrgInput(actorUserGuid);
        // Ensure the main person (inviter) is correctly set up
        var personInput = await CreatePersonAndAddToContext(actorUserGuid);

        var invitedUserModel1 = new InvitedApprovedUserModel { Person = new PersonModel { FirstName = "Inv1", LastName = "Inv1Last", ContactEmail = "invite1.inmem@test.com", TelephoneNumber = "111" }, JobTitle = "Approver1" };
        accountInput.InvitedApprovedUsers.Add(invitedUserModel1);

        var mainEnrolmentMapped = SetupMockMainEnrolmentForMapping(accountInput, personInput);
        mainEnrolmentMapped.Connection.Person.Email = personInput.Email; // Critical for inviter email
        _mockReExEnrolmentMaps.Setup(m => m.GetAdminEnrolmentForCurrentUser(accountInput, personInput))
            .Returns(mainEnrolmentMapped);

        _mockReExEnrolmentMaps.Setup(m =>
                m.GetApprovedPersonEnrolmentForCurrentUser(accountInput, personInput,
                    It.IsAny<PersonOrganisationConnection>()))
            .Returns(new Enrolment());

        var token1 = "INMEM_TOKEN_1";
        // Pass the Organisation object that will be tracked by EF Core (from mainEnrolmentMapped)
        var invitedEnrolmentMapped1 = SetupMockInvitedEnrolmentForMapping(invitedUserModel1, mainEnrolmentMapped.Connection.Organisation, personInput.Email, token1);
        _mockReExEnrolmentMaps.Setup(m => m.GetEnrolmentForInvitedApprovedUser(
            mainEnrolmentMapped.Connection.Organisation,
            invitedUserModel1,
            personInput.Email,
            It.IsAny<Person>()))
            .Returns(invitedEnrolmentMapped1);
        _mockTokenService.Setup(s => s.GenerateInviteToken()).Returns(token1);

        // Act
        await _accountService.AddReprocessorExporterOrganisationAsync(
            accountInput, personInput, ImmutableDictionary<string, PartnerRole>.Empty, ServiceKey, actorUserGuid);

        // Assert
        var savedMainOrganisation = await _dbContext.Organisations
            .FirstOrDefaultAsync(o => o.ExternalId == mainEnrolmentMapped.Connection.Organisation.ExternalId, default);
        savedMainOrganisation.Should().NotBeNull();

        var savedInvitedEnrolments = await _dbContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection.Person.User)
            .Include(e => e.Connection.Organisation)
            .Where(e => e.EnrolmentStatusId == EnrolmentStatus.Invited && e.Connection.OrganisationId == savedMainOrganisation.Id)
            .ToListAsync(default);
        savedInvitedEnrolments.Should().HaveCount(1);
        savedInvitedEnrolments[0].Connection.Person.Email.Should().Be("invite1.inmem@test.com");
        savedInvitedEnrolments[0].Connection.Person.User.InviteToken.Should().Be(token1);
        savedInvitedEnrolments[0].Connection.Person.User.InvitedBy.Should().Be(personInput.Email);
        savedInvitedEnrolments[0].ServiceRoleId.Should().Be(ServiceRole.ReprocessorExporter.ApprovedPerson.Id);
    }
}