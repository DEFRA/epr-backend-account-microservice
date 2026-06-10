using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace BackendAccountService.Data.IntegrationTests;

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class AccountServiceTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
{
    private readonly PerClassDbFixture _db;

    private AccountsDbContext _context = null!;

    public AccountServiceTests(PerClassDbFixture db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        _context = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_db.ConnectionString)
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options);

        //todo: this should be await _context.Database.MigrateAsync(), but switching it over fails the integration tests
        await _context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task WhenSavingEnrolmentData_OrganisationReferenceNumberIsSetToFirstAvailable()
    {
        await using var context = _context;

        var accountService = new AccountService(_context, new Mock<ITokenService>().Object, new Mock<IReExEnrolmentMaps>().Object);

        // First account data
        var account1 = RandomModelData.GetAccountModel(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

        var serviceRole1 = await accountService.GetServiceRoleAsync(account1.Connection.ServiceRole);

        var addedEnrolment = await accountService.AddAccountAsync(account1, serviceRole1!);

        addedEnrolment.Connection.Organisation.ReferenceNumber.Should().Be("100001");

        (await _context.Organisations.FirstAsync(default)).ReferenceNumber.Should().Be("100001");


        // Second account data
        var account2 = RandomModelData.GetAccountModel(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

        var serviceRole2 = await accountService.GetServiceRoleAsync(account2.Connection.ServiceRole);

        addedEnrolment = await accountService.AddAccountAsync(account2, serviceRole2!);

        addedEnrolment.Connection.Organisation.ReferenceNumber.Should().Be("100002");

        (await _context.Organisations.FirstAsync(organisation => organisation.ReferenceNumber == "100002", default)).Should().NotBeNull();
    }
}
