using System.Text.Json;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.IntegrationTests.AuditLogs;

public abstract class AuditLogsBaseTests
{
    protected static readonly Guid UserCreatingEnrolment = Guid.NewGuid();
    protected static readonly Guid OrganisationCreatingEnrolment = Guid.NewGuid();
    protected static readonly Guid UserRejectingEnrolment = Guid.NewGuid();
    protected static readonly Guid OrganisationRejectingEnrolment = Guid.NewGuid();
    protected static readonly Guid UserDeletingEnrolment = Guid.NewGuid();
    protected static readonly Guid OrganisationDeletingEnrolment = Guid.NewGuid();

    protected DbContextOptions<AccountsDbContext> Options = null!;
    protected Enrolment Enrolment = null!;

    protected static Enrolment CreateEnrolment() => new()
    {
        Connection = new PersonOrganisationConnection
        {
            JobTitle = "Owner",
            Organisation = new Organisation
            {
                ExternalId = OrganisationCreatingEnrolment,
                OrganisationTypeId = DbConstants.OrganisationType.CompaniesHouseCompany,
                ProducerTypeId = DbConstants.ProducerType.NotSet,
                CompaniesHouseNumber = "AB123456",
                Name = "Acme Corporation",
                BuildingName = "Best Building",
                BuildingNumber = "1",
                Street = "Best Street",
                Town = "Best Town",
                Postcode = "SW1A 2AA",
                NationId = DbConstants.Nation.England
            },
            OrganisationRoleId = DbConstants.OrganisationRole.Employer,
            Person = new Person
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@example.com",
                Telephone = "555-0100",
                User = new User
                {
                    UserId = UserCreatingEnrolment,
                    Email = "john.smith@example.com"
                }
            },
            PersonRoleId = DbConstants.PersonRole.Admin
        },
        EnrolmentStatusId = DbConstants.EnrolmentStatus.Pending
    };

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldSetTimeStamps()
    {
        await using var context = new AccountsDbContext(Options);

        var organisation = await context.Organisations.SingleAsync();

        organisation.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        organisation.LastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldCreateAddedAuditLogs()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLogs = await context.AuditLogs
            .Where(log => log.Operation == "Added")
            .ToListAsync();

        auditLogs.Should().NotBeEmpty().And.HaveCount(5)
            .And.ContainSingle(log => log.Entity == "Enrolment")
            .And.ContainSingle(log => log.Entity == "PersonOrganisationConnection")
            .And.ContainSingle(log => log.Entity == "Organisation")
            .And.ContainSingle(log => log.Entity == "Person")
            .And.ContainSingle(log => log.Entity == "User");
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldCreateModifiedAuditLogs()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLogs = await context.AuditLogs
            .Where(log => log.Operation == "Modified")
            .ToListAsync();

        auditLogs.Should().NotBeEmpty().And.HaveCount(1)
            .And.ContainSingle(log => log.Entity == "Enrolment");
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldCreateDeletedAuditLogs()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLogs = await context.AuditLogs
            .Where(log => log.Operation == "Deleted")
            .ToListAsync();

        auditLogs.Should().NotBeEmpty().And.HaveCount(1)
            .And.ContainSingle(log => log.Entity == "Enrolment");
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldSetPropertiesForAddedAuditLog()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLog = await context.AuditLogs
            .Where(log => log.Operation == "Added")
            .Where(log => log.Entity == "Person")
            .SingleAsync();

        auditLog.UserId.Should().Be(UserCreatingEnrolment);
        auditLog.OrganisationId.Should().Be(OrganisationCreatingEnrolment);
        auditLog.ServiceId.Should().BeNull();
        auditLog.Timestamp.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        auditLog.InternalId.Should().Be(Enrolment.Connection.Person.Id);
        auditLog.ExternalId.Should().Be(Enrolment.Connection.Person.ExternalId);
        auditLog.OldValues.Should().BeNull();
        auditLog.NewValues.Should().NotBeNull();
        auditLog.Changes.Should().BeNull();

        var newPerson = JsonSerializer.Deserialize<Person>(auditLog.NewValues!)!;
        newPerson.Id.Should().Be(Enrolment.Connection.Person.Id);
        newPerson.FirstName.Should().Be("John");
        newPerson.LastName.Should().Be("Smith");
        newPerson.Email.Should().Be("john.smith@example.com");
        newPerson.Telephone.Should().Be("555-0100");
        newPerson.UserId.Should().Be(Enrolment.Connection.Person.UserId);
        newPerson.ExternalId.Should().Be(Enrolment.Connection.Person.ExternalId);
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldSetPropertiesForModifiedAuditLog()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLog = await context.AuditLogs
            .Where(log => log.Operation == "Modified")
            .Where(log => log.Entity == "Enrolment")
            .SingleAsync();

        auditLog.UserId.Should().Be(UserRejectingEnrolment);
        auditLog.OrganisationId.Should().Be(OrganisationRejectingEnrolment);
        auditLog.ServiceId.Should().BeNull();
        auditLog.Timestamp.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        auditLog.InternalId.Should().Be(Enrolment.Id);
        auditLog.ExternalId.Should().Be(Enrolment.ExternalId);
        auditLog.OldValues.Should().NotBeNull();
        auditLog.NewValues.Should().NotBeNull();
        auditLog.Changes.Should().NotBeNull();

        var oldEnrolment = JsonSerializer.Deserialize<Enrolment>(auditLog.OldValues!)!;
        oldEnrolment.Id.Should().Be(Enrolment.Id);
        oldEnrolment.ConnectionId.Should().Be(Enrolment.ConnectionId);
        oldEnrolment.ServiceRoleId.Should().Be(Enrolment.ServiceRoleId);
        oldEnrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Pending);
        oldEnrolment.ExternalId.Should().Be(Enrolment.ExternalId);

        var newEnrolment = JsonSerializer.Deserialize<Enrolment>(auditLog.NewValues!)!;
        newEnrolment.Id.Should().Be(Enrolment.Id);
        newEnrolment.ConnectionId.Should().Be(Enrolment.ConnectionId);
        newEnrolment.ServiceRoleId.Should().Be(Enrolment.ServiceRoleId);
        newEnrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Rejected);
        newEnrolment.ExternalId.Should().Be(Enrolment.ExternalId);

        var changes = JsonSerializer.Deserialize<string[]>(auditLog.Changes!)!;
        changes.Should().Contain("EnrolmentStatusId");
    }

    [TestMethod]
    public async Task AccountsDbContext_SaveChangesAsync_ShouldSetPropertiesForDeletedAuditLog()
    {
        await using var context = new AccountsDbContext(Options);

        var auditLog = await context.AuditLogs
            .Where(log => log.Operation == "Deleted")
            .Where(log => log.Entity == "Enrolment")
            .SingleAsync();

        auditLog.UserId.Should().Be(UserDeletingEnrolment);
        auditLog.OrganisationId.Should().Be(OrganisationDeletingEnrolment);
        auditLog.ServiceId.Should().BeNull();
        auditLog.Timestamp.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        auditLog.InternalId.Should().Be(Enrolment.Id);
        auditLog.ExternalId.Should().Be(Enrolment.ExternalId);
        auditLog.OldValues.Should().NotBeNull();
        auditLog.NewValues.Should().BeNull();
        auditLog.Changes.Should().BeNull();

        var oldEnrolment = JsonSerializer.Deserialize<Enrolment>(auditLog.OldValues!)!;
        oldEnrolment.Id.Should().Be(Enrolment.Id);
        oldEnrolment.ConnectionId.Should().Be(Enrolment.ConnectionId);
        oldEnrolment.ServiceRoleId.Should().Be(Enrolment.ServiceRoleId);
        oldEnrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Rejected);
        oldEnrolment.ExternalId.Should().Be(Enrolment.ExternalId);
    }
}