using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using EnrolmentStatus = BackendAccountService.Core.Models.EnrolmentStatus;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [TestClass]
    public class GettingConnectionWithEnrolmentsFromOrganisationForServiceTests
    {
        private static AzureSqlDbContainer _database = null!;
        private AccountsDbContext _context = null!;
        private RoleManagementService _connectionsService = null!;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext _)
        {
            _database = await AzureSqlDbContainer.StartDockerDbAsync();
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

            await _context.Database.MigrateAsync();

            _connectionsService = new RoleManagementService(_context, 
                new ValidationService(_context, NullLogger<ValidationService>.Instance));
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        [DataRow(DbConstants.EnrolmentStatus.Enrolled, EnrolmentStatus.Enrolled)]
        [DataRow(DbConstants.EnrolmentStatus.Pending, EnrolmentStatus.Pending)]
        [DataRow(DbConstants.EnrolmentStatus.Approved, EnrolmentStatus.Approved)]
        [DataRow(DbConstants.EnrolmentStatus.Invited, EnrolmentStatus.Invited)]
        [DataRow(DbConstants.EnrolmentStatus.Nominated, EnrolmentStatus.Nominated)]
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

        [TestMethod]
        [DataRow(DbConstants.EnrolmentStatus.NotSet)]
        [DataRow(DbConstants.EnrolmentStatus.Rejected)]
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
