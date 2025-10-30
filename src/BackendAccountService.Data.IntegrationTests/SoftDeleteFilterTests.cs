using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]
public class SoftDeleteFilterTests
{
    private static AzureSqlDbContainer _database = null!;
    private static DbContextOptions<AccountsDbContext> _options = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlDbContainer.StartDockerDbAsync();
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_database.ConnectionString!, sqlServerOptionsAction: sqlOptions =>
                {
                    // using Polly would give better results, but this is nice and simple
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan
                            .FromSeconds(30),
                        errorNumbersToAdd: null
                    );
                })
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(_options);
        await context.Database.EnsureCreatedAsync();

        context.PersonOrganisationConnections.Add(new PersonOrganisationConnection
        {
            Person = MockEntity(new Person { User = MockEntity(new User { IsDeleted = true }), IsDeleted = true }),
            Organisation = MockEntity(new Organisation { IsDeleted = true }),
            IsDeleted = true
        });
        context.PersonOrganisationConnections.Add(new PersonOrganisationConnection
        {
            Person = MockEntity(new Person { User = MockEntity(new User { IsDeleted = false }), IsDeleted = false }),
            Organisation = MockEntity(new Organisation { IsDeleted = false }),
            IsDeleted = false
        });

        await context.SaveChangesAsync(Guid.Empty, Guid.Empty);
    }

    [ClassCleanup]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }

    private static User MockEntity(User user)
    {
        user.UserId = Guid.NewGuid();
        return user;
    }

    private static Person MockEntity(Person person)
    {
        person.FirstName = "John";
        person.LastName = "Smith";
        person.Email = "john.smith@acme.com";
        person.Telephone = "5550100";
        return person;
    }

    private static Organisation MockEntity(Organisation organisation)
    {
        organisation.Name = "Acme Corporation";
        return organisation;
    }

    [TestMethod]
    public async Task AccountsDbContext_ShouldNotReturnSoftDeletedEntitiesByDefault()
    {
        await using var context = new AccountsDbContext(_options);

        var usersList = await context.Users.ToListAsync();
        var personsList = await context.Persons.ToListAsync();
        var organisationsList = await context.Organisations.ToListAsync();
        var connectionsList = await context.PersonOrganisationConnections.ToListAsync();

        usersList.Should().NotBeEmpty();
        usersList.Should().OnlyContain(user => !user.IsDeleted);

        personsList.Should().NotBeEmpty();
        personsList.Should().OnlyContain(person => !person.IsDeleted);

        organisationsList.Should().NotBeEmpty();
        organisationsList.Should().OnlyContain(organisation => !organisation.IsDeleted);

        connectionsList.Should().NotBeEmpty();
        connectionsList.Should().OnlyContain(connection => !connection.IsDeleted);
    }

    [TestMethod]
    public async Task AccountsDbContext_ShouldReturnSoftDeletedEntitiesWhenIgnoringFilters()
    {
        await using var context = new AccountsDbContext(_options);

        var usersList = await context.Users.IgnoreQueryFilters().ToListAsync();
        var personsList = await context.Persons.IgnoreQueryFilters().ToListAsync();
        var organisationsList = await context.Organisations.IgnoreQueryFilters().ToListAsync();
        var connectionsList = await context.PersonOrganisationConnections.AsNoTracking().IgnoreQueryFilters().ToListAsync();

        usersList.Should().NotBeEmpty();
        usersList.Should().Contain(user => user.IsDeleted);
        usersList.Should().Contain(user => !user.IsDeleted);

        personsList.Should().NotBeEmpty();
        personsList.Should().Contain(person => person.IsDeleted);
        personsList.Should().Contain(person => !person.IsDeleted);

        organisationsList.Should().NotBeEmpty();
        organisationsList.Should().Contain(organisation => organisation.IsDeleted);
        organisationsList.Should().Contain(organisation => !organisation.IsDeleted);

        connectionsList.Should().NotBeEmpty();
        connectionsList.Should().Contain(connection => connection.IsDeleted);
        connectionsList.Should().Contain(connection => !connection.IsDeleted);
    }
}