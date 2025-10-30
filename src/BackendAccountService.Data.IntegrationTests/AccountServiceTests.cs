using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]
public class AccountServiceTests
{
    private static AzureSqlDbContainer _database = null!;

    private AccountsDbContext _context = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlDbContainer.StartDockerDbAsync();
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

        //todo: this should be await _context.Database.MigrateAsync(), but switching it over fails the integration tests
        await _context.Database.EnsureCreatedAsync();
    }

    [TestMethod]
    public async Task WhenSavingEnrolmentData_OrganisationReferenceNumberIsSetToFirstAvailable()
    {
        await using var context = _context;

        var accountService = new AccountService(_context, new Mock<ITokenService>().Object, new Mock<IReExEnrolmentMaps>().Object);

        // First account data
        var account1 = RandomModelData.GetAccountModel(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

        var serviceRole1 = await accountService.GetServiceRoleAsync(account1.Connection.ServiceRole);

        var addedEnrolment = await accountService.AddAccountAsync(account1, serviceRole1!);

        addedEnrolment.Connection.Organisation.ReferenceNumber.Should().Be("100001");

        (await _context.Organisations.FirstAsync()).ReferenceNumber.Should().Be("100001");


        // Second account data
        var account2 = RandomModelData.GetAccountModel(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

        var serviceRole2 = await accountService.GetServiceRoleAsync(account2.Connection.ServiceRole);

        addedEnrolment = await accountService.AddAccountAsync(account2, serviceRole2!);

        addedEnrolment.Connection.Organisation.ReferenceNumber.Should().Be("100002");

        (await _context.Organisations.FirstAsync(organisation => organisation.ReferenceNumber == "100002")).Should().NotBeNull();
    }
}
