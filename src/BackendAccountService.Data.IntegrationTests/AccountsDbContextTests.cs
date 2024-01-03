using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]
public class AccountsDbContextTests
{
    private static AzureSqlEdgeDbContainer _database = null!;

    private AccountsDbContext _context = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlEdgeDbContainer.StartDockerDbAsync();
    }

    [ClassCleanup]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }

    [TestInitialize]
    public async Task Setup()
    {
        _context = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_database.ConnectionString)
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options);

        await _context.Database.EnsureCreatedAsync();
    }

    [TestMethod]
    public async Task WhenDatabaseDoesNotExist_ThenItGetsCreatedWithEnsureCreated()
    {
        await using var context = _context;

        var didExist = await context.Database.EnsureDeletedAsync();

        var isCreated = await context.Database.EnsureCreatedAsync();

        Assert.IsTrue(didExist);

        Assert.IsTrue(isCreated);
    }

    [TestMethod]
    public async Task WhenSavingComplianceSchemaData_ThenComplianceSchemeAndOrganisationTablesGetPopulated()
    {
        await using var context = _context;

        var companiesHouseNumber = "345456456";

        var organisation = new Organisation
        {
            OrganisationTypeId = context.OrganisationTypes.First().Id,
            Name = "Compliance-scheme-operator-1",
            Town = "Town1",
            Postcode = "Post",
            ValidatedWithCompaniesHouse = false,
            IsComplianceScheme = true,
            CompaniesHouseNumber = companiesHouseNumber
        };
        
        var complianceScheme = new ComplianceScheme
        {
            Name = "Compliance-scheme-1",
            CompaniesHouseNumber = companiesHouseNumber
        };

        context.ComplianceSchemes.Add(complianceScheme);
        context.Organisations.Add(organisation);

        await context.SaveChangesAsync(Guid.Empty, Guid.Empty);

        Assert.IsNotNull(context.ComplianceSchemes.Single(s => s.Name == "Compliance-scheme-1"));
        Assert.IsNotNull(context.Organisations.Single(o => o.Name == "Compliance-scheme-operator-1"));
    }

    [TestMethod]
    public async Task WhenSavingPersonData_ThenPersonsAndUsersTablesGetPopulated()
    {
        await using var context = _context;

        var person = new Person
        {
            Email = "person-one",
            FirstName = "person",
            LastName = "one",
            Telephone = "0123456789",
            User = new User
            {
                UserId = Guid.NewGuid(),
                Email = "user-one",
                ExternalIdpId = "external ID Provider",
                ExternalIdpUserId = "External ID Provider ID 1234"
            }
        };

        context.Persons.Add(person);

        await context.SaveChangesAsync(person.User.UserId.Value, Guid.Empty);

        Assert.IsNotNull(context.Users.First(o => o.Email == "user-one").Person);

        Assert.IsNotNull(context.Persons.First(o => o.Email == "person-one").User);
    }

    [TestMethod]
    public async Task WhenSavingUserData_ThenPersonsAndUsersTablesGetPopulated()
    {
        await using var context = _context;

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user-two",
            ExternalIdpId = "external ID Provider",
            ExternalIdpUserId = "External ID Provider ID 1234",
            Person = new Person
            {
                Email = "person-two",
                FirstName = "person",
                LastName = "two",
                Telephone = "0123456789"
            }
        };

        context.Users.Add(newUser);

        await context.SaveChangesAsync(newUser.UserId.Value, Guid.Empty);

        var person = context.Users
            .Where(o => o.Email == "user-two").Include(user => user.Person).First()
            .Person;
        
        person.Should().NotBeNull();

        var user = context.Persons
            .Where(o => o.Email == "person-two").Include(person => person.User).First()
            .User;

        user.Should().NotBeNull();
    }

    [TestMethod]
    public async Task WhenSavingUserData_ShouldAllowDuplicateEmptyUserIds()
    {
        await using var context = _context;

        var user1 = new User
        {
            UserId = Guid.Empty,
            Email = "user-one",
            ExternalIdpId = "external ID Provider",
            ExternalIdpUserId = "External ID Provider ID 1234",
            Person = new Person
            {
                Email = "person-one",
                FirstName = "person",
                LastName = "one",
                Telephone = "0123456789"
            }
        };

        var user2 = new User
        {
            UserId = Guid.Empty,
            Email = "user-two",
            ExternalIdpId = "external ID Provider",
            ExternalIdpUserId = "External ID Provider ID 1234",
            Person = new Person
            {
                Email = "person-two",
                FirstName = "person",
                LastName = "two",
                Telephone = "0123456789"
            }
        };

        context.Users.AddRange(user1, user2);

        await context.SaveChangesAsync(Guid.Empty, Guid.Empty);

        context.Users.Count(u => u.UserId == Guid.Empty).Should().Be(2);
    }

    [TestMethod]
    public async Task WhenSavingUserData_ShouldNotAllowDuplicateNonemptyUserIds()
    {
        await using var context = _context;

        var userId = Guid.NewGuid();

        var user1 = new User
        {
            UserId = userId,
            Email = "user-one",
            ExternalIdpId = "external ID Provider",
            ExternalIdpUserId = "External ID Provider ID 1234",
            Person = new Person
            {
                Email = "person-one",
                FirstName = "person",
                LastName = "one",
                Telephone = "0123456789"
            }
        };

        var user2 = new User
        {
            UserId = userId,
            Email = "user-two",
            ExternalIdpId = "external ID Provider",
            ExternalIdpUserId = "External ID Provider ID 1234",
            Person = new Person
            {
                Email = "person-two",
                FirstName = "person",
                LastName = "two",
                Telephone = "0123456789"
            }
        };

        context.Users.AddRange(user1, user2);

        var action = async () => await context.SaveChangesAsync(Guid.Empty, Guid.Empty);

        await action.Should().ThrowExactlyAsync<DbUpdateException>().WithInnerException<DbUpdateException, SqlException>().WithMessage("Cannot insert duplicate key*");
    }
}
