using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests.AuditLogs;

[TestClass]
public class AuditLogsTransactionSyncTests : AuditLogsBaseTests
{
    private static AzureSqlDbContainer _database = null!;
    private static DbContextOptions<AccountsDbContext> _options = null!;

    private new static readonly Enrolment Enrolment = CreateEnrolment();

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlDbContainer.StartDockerDbAsync();
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_database.ConnectionString!)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(_options);
        await context.Database.EnsureCreatedAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        var serviceRole = await context.ServiceRoles.SingleAsync(role => role.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
        Enrolment.ServiceRoleId = serviceRole.Id;
        context.Add(Enrolment);
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(UserCreatingEnrolment, OrganisationCreatingEnrolment);

        Enrolment.EnrolmentStatusId = DbConstants.EnrolmentStatus.Rejected;
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(UserRejectingEnrolment, OrganisationRejectingEnrolment);

        context.Remove(Enrolment);
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(UserDeletingEnrolment, OrganisationDeletingEnrolment);

        await transaction.CommitAsync();
    }

    [TestInitialize]
    public void TestSetup()
    {
        Options = _options;
        base.Enrolment = Enrolment;
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }
}