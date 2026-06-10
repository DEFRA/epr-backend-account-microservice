using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests.AuditLogs;

/// <summary>
/// Per-class fixture that prepares the DB and seeds the audit-log scenario via the sync
/// SaveChanges overload, without an explicit transaction.
/// </summary>
public class AuditLogsNoTransactionSyncFixture : PerClassDbFixture
{
    public DbContextOptions<AccountsDbContext> Options { get; private set; } = null!;
    public Enrolment Enrolment { get; } = AuditLogsBaseTests.CreateAuditEnrolment();

    public AuditLogsNoTransactionSyncFixture(SharedSqlFixture shared) : base(shared) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(ConnectionString)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(Options);
        await context.Database.EnsureCreatedAsync(default);

        var serviceRole = await context.ServiceRoles.SingleAsync(role => role.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, cancellationToken: default);
        Enrolment.ServiceRoleId = serviceRole.Id;
        context.Add(Enrolment);
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(AuditLogsBaseTests.UserCreatingEnrolment, AuditLogsBaseTests.OrganisationCreatingEnrolment);

        Enrolment.EnrolmentStatusId = DbConstants.EnrolmentStatus.Rejected;
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(AuditLogsBaseTests.UserRejectingEnrolment, AuditLogsBaseTests.OrganisationRejectingEnrolment);

        context.Remove(Enrolment);
        // ReSharper disable once MethodHasAsyncOverload
        context.SaveChanges(AuditLogsBaseTests.UserDeletingEnrolment, AuditLogsBaseTests.OrganisationDeletingEnrolment);
    }
}

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class AuditLogsNoTransactionSyncTests : AuditLogsBaseTests, IClassFixture<AuditLogsNoTransactionSyncFixture>
{
    public AuditLogsNoTransactionSyncTests(AuditLogsNoTransactionSyncFixture fixture)
    {
        Options = fixture.Options;
        Enrolment = fixture.Enrolment;
    }
}
