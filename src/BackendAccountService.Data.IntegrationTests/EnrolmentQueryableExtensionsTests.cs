using System.Diagnostics;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]

public class EnrolmentQueryableExtensionsTests
{
    private static AzureSqlDbContainer _database = null!;
    private static DbContextOptions<AccountsDbContext> _options = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlDbContainer.StartDockerDbAsync();
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_database.ConnectionString!)
            .LogTo(message => Debug.WriteLine(message))
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(_options);
        await context.Database.EnsureCreatedAsync(default);
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }

    [TestMethod]
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
        
        Assert.IsNull(organisation);
    }
}