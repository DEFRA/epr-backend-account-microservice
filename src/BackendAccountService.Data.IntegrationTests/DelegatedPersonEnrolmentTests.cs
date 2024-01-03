using System.Diagnostics;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Data.IntegrationTests;

[TestClass]
public class DelegatedPersonEnrolmentTests
{
    private static AzureSqlEdgeDbContainer _database = null!;
    private static DbContextOptions<AccountsDbContext> _options = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlEdgeDbContainer.StartDockerDbAsync();
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_database.ConnectionString!)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new AccountsDbContext(_options);
        await context.Database.EnsureCreatedAsync();
    }

    [ClassCleanup]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }

    [TestMethod]
    public async Task WhenReadingSavedDelegatedPersonEnrolment_AllDelegatedPersonDetailsAreReturnedCorrectlySet()
    {
        await using var context = new AccountsDbContext(_options);

        var organisationId = Guid.NewGuid();

        var approvedPersonUserId = Guid.NewGuid();

        var inviterEnrolment = context.Enrolments.Add(new Enrolment
        {
            Connection = new PersonOrganisationConnection
            {
                JobTitle = "the-owner-title",
                Organisation = new Organisation
                {
                    ExternalId = organisationId,
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
                Person = CreatePerson("the-owner", approvedPersonUserId),
                PersonRoleId = DbConstants.PersonRole.Admin
            },
            EnrolmentStatusId = DbConstants.EnrolmentStatus.Enrolled,
            ServiceRoleId = DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        });

        await context.SaveChangesAsync(approvedPersonUserId, organisationId);

        var basicUserEnrolment = context.Enrolments.Add(new Enrolment
        {
            Connection = new PersonOrganisationConnection
            {
                JobTitle = "some-employee-title",
                OrganisationId = inviterEnrolment.Entity.Connection.OrganisationId,
                OrganisationRoleId = DbConstants.OrganisationRole.Employer,
                PersonRoleId = DbConstants.PersonRole.Admin,
                Person = CreatePerson("some-employee", Guid.NewGuid()),
            },
            EnrolmentStatusId = DbConstants.EnrolmentStatus.Enrolled,
            ServiceRoleId = DbConstants.ServiceRole.Packaging.BasicUser.Id
        });

        await context.SaveChangesAsync(approvedPersonUserId, organisationId);

        context.DelegatedPersonEnrolments.Add(new DelegatedPersonEnrolment()
        {
            ComplianceSchemeName = "A Compliance Scheme",
            EnrolmentId = basicUserEnrolment.Entity.Id,
            NominatorEnrolmentId = inviterEnrolment.Entity.Id,
            RelationshipType = RelationshipType.ComplianceScheme
        });

        await context.SaveChangesAsync(approvedPersonUserId, organisationId);

        var basicUserEnrolmentId = basicUserEnrolment.Entity.Id;

        var checkedEnrolment = await context.DelegatedPersonEnrolments.SingleAsync(enrolment => enrolment.EnrolmentId == basicUserEnrolmentId);

        checkedEnrolment.Id.Should().NotBe(default);
        checkedEnrolment.EnrolmentId.Should().NotBe(default);
        checkedEnrolment.EnrolmentId.Should().Be(basicUserEnrolment.Entity.Id);
        checkedEnrolment.NominatorEnrolmentId.Should().NotBe(default);
        checkedEnrolment.NominatorEnrolmentId.Should().Be(inviterEnrolment.Entity.Id);
        checkedEnrolment.NominatorEnrolmentId.Should().NotBe(checkedEnrolment.EnrolmentId);
        checkedEnrolment.NomineeDeclaration.Should().BeNull();
        checkedEnrolment.NomineeDeclarationTime.Should().BeNull();
        checkedEnrolment.NominatorDeclaration.Should().BeNull();
        checkedEnrolment.NominatorDeclarationTime.Should().BeNull();
        checkedEnrolment.ConsultancyName.Should().BeNull();
        checkedEnrolment.ComplianceSchemeName.Should().Be("A Compliance Scheme");
        checkedEnrolment.OtherOrganisationName.Should().BeNull();
        checkedEnrolment.RelationshipType.Should().Be(RelationshipType.ComplianceScheme);
        checkedEnrolment.OtherRelationshipDescription.Should().BeNull();
        checkedEnrolment.CreatedOn.Should().NotBe(default);
        checkedEnrolment.LastUpdatedOn.Should().NotBe(default);
        checkedEnrolment.IsDeleted.Should().BeFalse();

        checkedEnrolment.LastUpdatedOn.Should().BeExactly(checkedEnrolment.CreatedOn);

        checkedEnrolment.IsDeleted = true;
        await context.SaveChangesAsync(approvedPersonUserId, organisationId);
        checkedEnrolment.LastUpdatedOn.Should().BeAfter(checkedEnrolment.CreatedOn);

        var deletedEnrolment = await context.DelegatedPersonEnrolments.SingleOrDefaultAsync(enrolment => enrolment.EnrolmentId == basicUserEnrolmentId);
        deletedEnrolment.Should().BeNull();

        deletedEnrolment = await context.DelegatedPersonEnrolments.IgnoreQueryFilters().SingleOrDefaultAsync(enrolment => enrolment.EnrolmentId == basicUserEnrolmentId);
        deletedEnrolment.Should().NotBeNull();
    }

    private static Person CreatePerson(string label, Guid userId) => new()
    {
        FirstName = $"{label} First Name",
        LastName = $"{label} Last Name",
        Email = $"{label}@example.com",
        Telephone = "555-0100",
        User = new User
        {
            UserId = userId,
            Email = $"{label}@example.com"
        }
    };
}
