using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using EnrolmentStatus = BackendAccountService.Core.Models.EnrolmentStatus;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [Collection(SharedSqlCollection.Name)]
    [Trait("Category", "IntegrationTest")]
    public class GettingConnectionWithEnrolmentsFromOrganisationForServiceTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
    {
        private readonly PerClassDbFixture _db;
        private AccountsDbContext _context = null!;
        private RoleManagementService _connectionsService = null!;

        public GettingConnectionWithEnrolmentsFromOrganisationForServiceTests(PerClassDbFixture db)
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

            await _context.Database.MigrateAsync();

            _connectionsService = new RoleManagementService(_context,
                new ValidationService(_context, NullLogger<ValidationService>.Instance));
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task WhenUserTriesToAccessNonExistingConnection_ThenTheyReceiveNullResponse()
        {
            await using var context = _context;

            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Guid connectionId = Guid.NewGuid();

            authorisedPersonEnrolment.Should().NotBe(authorisedPersonEnrolment.Connection.ExternalId);

            var enrolments = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, organisationId, "Packaging");

            enrolments.Should().BeNull();
        }

        [Fact]
        public async Task WhenUserTriesToAccessConnectionFromDifferentOrganisation_ThenTheyReceiveNullResponse()
        {
            await using var context = _context;

            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

            Guid otherOrganisationId = Guid.NewGuid();

            var enrolments = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, otherOrganisationId, "Packaging");

            enrolments.Should().BeNull();
        }

        [Fact]
        public async Task WhenUserTriesToAccessEnrolmentsFromDifferentService_ThenTheyReceiveNoEnrolments()
        {
            await using var context = _context;

            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

            var enrolments = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, organisationId, "Some Other Service - Not Packaging");

            enrolments.Should().NotBeNull();
            enrolments.PersonRole.Should().Be(PersonRole.Admin);
            enrolments.Enrolments.Should().BeEmpty();
        }

        [Theory]
        [InlineData(DbConstants.EnrolmentStatus.Enrolled, EnrolmentStatus.Enrolled)]
        [InlineData(DbConstants.EnrolmentStatus.Pending, EnrolmentStatus.Pending)]
        [InlineData(DbConstants.EnrolmentStatus.Approved, EnrolmentStatus.Approved)]
        [InlineData(DbConstants.EnrolmentStatus.Invited, EnrolmentStatus.Invited)]
        [InlineData(DbConstants.EnrolmentStatus.Nominated, EnrolmentStatus.Nominated)]
        public async Task WhenThereAreMatchingActiveEnrolments_ThenConnectionWithEnrolmentsAreReturned(int activeEnrolmentStatusId, EnrolmentStatus activeEnrolmentStatus)
        {
            await using var context = _context;

            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, activeEnrolmentStatusId);

            Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

            var enrolments = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, organisationId, "Packaging");

            enrolments.Should().NotBeNull();

            enrolments.PersonRole.Should().Be(PersonRole.Admin);
            enrolments.Enrolments.Should().HaveCount(1);
            enrolments.Enrolments.First().EnrolmentStatus.Should().Be(activeEnrolmentStatus);
            enrolments.Enrolments.First().ServiceRoleKey.Should().Be(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
        }

        [Theory]
        [InlineData(DbConstants.EnrolmentStatus.NotSet)]
        [InlineData(DbConstants.EnrolmentStatus.Rejected)]
        public async Task WhenThereAreMatchingNonActiveEnrolments_ThenReturnEmptyListOfEnrolments(int nonActiveEnrolmentStatus)
        {
            await using var context = _context;

            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, nonActiveEnrolmentStatus);

            Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

            var enrolments = await _connectionsService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, organisationId, "Packaging");

            enrolments.Should().NotBeNull();
            enrolments.PersonRole.Should().Be(PersonRole.Admin);
            enrolments.Enrolments.Should().BeEmpty();
        }
    }
}
