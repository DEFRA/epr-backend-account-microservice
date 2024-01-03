using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [TestClass]
    public class AcceptNominationToDelegatedPersonTests
    {
        private static AzureSqlEdgeDbContainer _database = null!;
        private static DbContextOptions<AccountsDbContext> _options = null!;
        private AccountsDbContext _writeDbContext = null!;
        private RoleManagementService _connectionsService = null!;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext _)
        {
            _database = await AzureSqlEdgeDbContainer.StartDockerDbAsync();

            _options = new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_database.ConnectionString)
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options;
        }

        [ClassCleanup]
        public static async Task TestFixtureTearDown()
        {
            await _database.StopAsync();
        }

        [TestInitialize]
        public async Task Setup()
        {
            _writeDbContext = new AccountsDbContext(_options);

            await _writeDbContext.Database.EnsureCreatedAsync();

            _connectionsService = new RoleManagementService(_writeDbContext,
                new ValidationService(_writeDbContext, NullLogger<ValidationService>.Instance));
        }

        [TestCleanup]
        public async Task TearDown()
        {
            await _writeDbContext.DisposeAsync();
        }

        [TestMethod]
        [TestCategory("AcceptNominationToDelegatedPerson")]
        public async Task WhenNominationIsAccepted_ThenEnrlomentStateChangesToPendingAndThereAreNoBasicUserEnrolments()
        {
            Guid organisationId = Guid.NewGuid();

            var approvedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

            await _connectionsService.NominateToDelegatedPerson(
                connectionId: enrolment.Connection.ExternalId,
                userId: approvedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
                {
                    RelationshipType = Entities.Conversions.RelationshipType.Employment,
                    JobTitle = "Nominee Job Title",
                    NominatorDeclaration = "Nominator Declaration"
                });

            var nominatedPersonEnrolment = await _writeDbContext.Enrolments
                .Where(nominated => nominated.Connection.Person.User.UserId == enrolment.Connection.Person.User.UserId)
                .Where(nominated => nominated.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
                .FirstOrDefaultAsync();

            var result = await _connectionsService.AcceptNominationToDelegatedPerson(
                enrolmentId: nominatedPersonEnrolment.ExternalId,
                organisationId: organisationId,
                userId: enrolment.Connection.Person.User.UserId.Value,
                serviceKey: "Packaging",
                acceptNominationRequest: new Core.Models.Request.AcceptNominationRequest
                {
                    NomineeDeclaration = "Nominee Declaration",
                    Telephone = "01234000000"
                });

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            await using var readDbContext = new AccountsDbContext(_options);

            var nominatedEnrolments = await readDbContext
                .Enrolments
                .Where(enrolment =>
                    enrolment.ConnectionId == enrolment.ConnectionId &&
                    enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
                .ToListAsync();

            nominatedEnrolments.Should().BeEmpty();

            var enrolments = await readDbContext
                .Enrolments
                .Where(enrolment => enrolment.ConnectionId == enrolment.ConnectionId)
                .Include(enrolment => enrolment.DelegatedPersonEnrolment)
                .Include(enrolment => enrolment.Connection.Person)
                .ToListAsync();

            enrolments.Where(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Pending).Should().HaveCount(1);

            var pendingEnrolment = enrolments.FirstOrDefault(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Pending);

            pendingEnrolment.Should().NotBeNull();

            pendingEnrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Pending);
            pendingEnrolment.DelegatedPersonEnrolment.RelationshipType.Should().Be(Entities.Conversions.RelationshipType.Employment);
            pendingEnrolment.DelegatedPersonEnrolment.NomineeDeclaration.Should().Be("Nominee Declaration");
            pendingEnrolment.DelegatedPersonEnrolment.NomineeDeclarationTime.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
            pendingEnrolment.DelegatedPersonEnrolment.NominatorDeclaration.Should().Be("Nominator Declaration");
            pendingEnrolment.DelegatedPersonEnrolment.NominatorDeclarationTime.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
            pendingEnrolment.Connection.JobTitle.Should().Be("Nominee Job Title");
            pendingEnrolment.Connection.Person.Telephone.Should().Be("01234000000");

            enrolments.Where(enrolment => enrolment.ServiceRoleId == DbConstants.ServiceRole.Packaging.BasicUser.Id).Should().BeEmpty();
        }
    }
}
