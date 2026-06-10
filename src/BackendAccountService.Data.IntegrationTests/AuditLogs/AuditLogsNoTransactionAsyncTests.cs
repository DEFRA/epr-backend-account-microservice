using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests.AuditLogs;

/// <summary>
/// Per-class fixture that prepares the DB and seeds the audit-log scenario via the async
/// SaveChangesAsync overload, without an explicit transaction.
/// </summary>
public class AuditLogsNoTransactionAsyncFixture : PerClassDbFixture
{
    public DbContextOptions<AccountsDbContext> Options { get; private set; } = null!;
    public Enrolment Enrolment { get; } = AuditLogsBaseTests.CreateAuditEnrolment();

    public AuditLogsNoTransactionAsyncFixture(SharedSqlFixture shared) : base(shared) { }

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

        var serviceRole = await context.ServiceRoles.SingleAsync(role => role.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, default);
        Enrolment.ServiceRoleId = serviceRole.Id;
        context.Add(Enrolment);
        await context.SaveChangesAsync(AuditLogsBaseTests.UserCreatingEnrolment, AuditLogsBaseTests.OrganisationCreatingEnrolment, default);

        Enrolment.EnrolmentStatusId = DbConstants.EnrolmentStatus.Rejected;
        await context.SaveChangesAsync(AuditLogsBaseTests.UserRejectingEnrolment, AuditLogsBaseTests.OrganisationRejectingEnrolment, default);

        context.Remove(Enrolment);
        await context.SaveChangesAsync(AuditLogsBaseTests.UserDeletingEnrolment, AuditLogsBaseTests.OrganisationDeletingEnrolment, default);
    }
}

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class AuditLogsNoTransactionAsyncTests : AuditLogsBaseTests, IClassFixture<AuditLogsNoTransactionAsyncFixture>
{
    public AuditLogsNoTransactionAsyncTests(AuditLogsNoTransactionAsyncFixture fixture)
    {
        Options = fixture.Options;
        Enrolment = fixture.Enrolment;
    }
}
