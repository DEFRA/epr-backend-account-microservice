using System.Diagnostics;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]
public class AccountServiceTests
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
    public async Task WhenSavingEnrolmentData_OrganisationReferenceNumberIsSetToFirstAvailable()
    {
        await using var context = _context;

        var accountService = new AccountService(_context);


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
