using System.Diagnostics;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.IntegrationTests;

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class EnrolmentQueryableExtensionsTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
{
    private readonly PerClassDbFixture _db;
    private DbContextOptions<AccountsDbContext> _options = null!;

    public EnrolmentQueryableExtensionsTests(PerClassDbFixture db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_db.ConnectionString)
            .LogTo(message => Debug.WriteLine(message))
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(_options);
        await context.Database.EnsureCreatedAsync(default);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EnrolmentsQueryableExtensions_GetOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var userObjectId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        await using var context = new AccountsDbContext(_options);

        var organisation = context.Enrolments
            .WhereServiceRoleIn(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
            .WhereEnrolmentStatusIn(DbConstants.EnrolmentStatus.Pending, DbConstants.EnrolmentStatus.Approved)
            .WhereOrganisationIsProducer()
            .WhereOrganisationIdIs(organisationId)
            .WhereUserObjectIdIs(userObjectId)
            .SelectDistinctSingleOrganisation();

        organisation.Should().BeNull();
    }
}
