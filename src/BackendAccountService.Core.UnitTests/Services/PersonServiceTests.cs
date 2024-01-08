using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class PersonServiceTests
{
    private AccountsDbContext _accountContext= null!;
    private PersonService _personService = null!;

    private readonly Guid UserId = Guid.NewGuid();
    private readonly Guid ExternalId = Guid.NewGuid();
    private readonly Guid NoneExistentUserId = Guid.NewGuid();
    private readonly Guid DeletedUserId = Guid.NewGuid();
    private readonly string PersonInviteToken = "some other invite token";
    private readonly string InvitedPersonEmail = "invited@person.com";

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("PersonServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _personService = new PersonService(_accountContext);
    }



    [TestMethod]
    public async Task GetPersonByUserIdAsync_WhenValidId_ThenReturnPerson()
    {
        var person = await _personService.GetPersonByUserIdAsync(UserId);

        var expectedPerson = await _accountContext.Persons.FirstOrDefaultAsync(p => p.User.UserId == UserId);

        person.Should().NotBeNull();
        person?.FirstName.Should().Be(expectedPerson?.FirstName);
        person?.LastName.Should().Be(expectedPerson?.LastName);
        person?.ContactEmail.Should().Be(expectedPerson?.Email);
        person?.TelephoneNumber.Should().Be(expectedPerson?.Telephone);
        person?.CreatedOn.Should().Be(expectedPerson?.CreatedOn);
    }

    [TestMethod]
    public async Task GetPersonByUserIdAsync_WhenInvalidId_ThenReturnNull()
    {
        var person = await _personService.GetPersonByUserIdAsync(NoneExistentUserId);

        person.Should().BeNull();
    }

    [TestMethod]
    public async Task GetPersonByUserIdAsync_WhenDeletedUserId_ThenReturnNull()
    {
        var person = await _personService.GetPersonByUserIdAsync(DeletedUserId);

        person.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetPersonByExternalIdAsync_WhenValid_ThenReturnPerson()
    {
        var person = await _personService.GetPersonByExternalIdAsync(ExternalId);

        var expectedPerson = await _accountContext.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ExternalId == ExternalId);

        person.Should().NotBeNull();
        person?.FirstName.Should().Be(expectedPerson?.FirstName);
        person?.LastName.Should().Be(expectedPerson?.LastName);
        person?.ContactEmail.Should().Be(expectedPerson?.Email);
        person?.TelephoneNumber.Should().Be(expectedPerson?.Telephone);
        person?.CreatedOn.Should().Be(expectedPerson?.CreatedOn);
    }
    
    [TestMethod]
    public async Task GetPersonServiceRoleByInviteTokenAsync_WhenInvalid_ThenReturnNull()
    {
        // act
        var person = await _personService.GetPersonServiceRoleByInviteTokenAsync(PersonInviteToken);

        // assert
        person.Should().BeNull();
    }

    private void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);
        
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var person = new Person
        {
             IsDeleted = false,
             LastName = "Ramone",
             FirstName= "Joey",
             Email = "joey@ramones.com",
             Telephone = "07904666666",
             User = new User
             {
                  Email = "joey@ramones.com",
                  UserId = UserId,
                  ExternalIdpId = "xxxxxx",
                  ExternalIdpUserId = "123456"
             }
        };
        setupContext.Persons.Add(person);

        var deletedPerson = new Person
        {
            IsDeleted = true,
            LastName = "Ramone",
            FirstName = "Tommy",
            Email = "tommy@ramones.com",
            Telephone = "07904777777",
            User = new User
            {
                Email = "tommy@ramones.com",
                UserId = DeletedUserId,
                ExternalIdpId = "xxxxxx",
                ExternalIdpUserId = "654321"
            }
        };
        setupContext.Persons.Add(deletedPerson);

        var invitedPersonId = 111;
        var invitedOrgId = 112;
        var invitedConnectionId = 113;
        var invitedPerson = new Person
        {
            Id = invitedPersonId,
            LastName = "InvitedPerson",
            FirstName = "Tommy",
            Email = InvitedPersonEmail,
            Telephone = "07904777777",
            User = new User
            {
                Email = InvitedPersonEmail,
                UserId = Guid.NewGuid(),
                ExternalIdpId = "xxxxxx",
                ExternalIdpUserId = "654321",
                InviteToken = PersonInviteToken
            }
        };
        setupContext.Persons.Add(invitedPerson);

        var invitedPersonConnection = new PersonOrganisationConnection
        {
            PersonId = invitedPersonId,
            OrganisationId = invitedOrgId,
            Id = invitedConnectionId
        };
        var invitedOrg = new Organisation { Id = invitedOrgId, Name = "some name"};
        var invitedEnrolment = new Enrolment { ConnectionId = invitedConnectionId }; 
        setupContext.PersonOrganisationConnections.Add(invitedPersonConnection);
        setupContext.Organisations.Add(invitedOrg);
        setupContext.Enrolments.Add(invitedEnrolment);
        
        var externalIdPerson = new Person
        {
            IsDeleted = false,
            LastName = "Smith",
            FirstName = "Ted",
            Email = "ted@smiths.com",
            ExternalId = ExternalId,
            Telephone = "",
            User = new User
            {
                Email = "ted@smiths.com",
                UserId = Guid.NewGuid()
            }
        };
        setupContext.Persons.Add(externalIdPerson);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
    
    
   
}
