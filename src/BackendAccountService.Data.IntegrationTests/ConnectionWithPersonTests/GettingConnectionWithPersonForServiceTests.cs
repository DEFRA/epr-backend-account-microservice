using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;
using EnrolmentStatus = BackendAccountService.Core.Models.EnrolmentStatus;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [TestClass]
    public class GettingConnectionWithPersonForServiceTests
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

            _connectionsService = new RoleManagementService(_context, new ValidationService(_context, NullLogger<ValidationService>.Instance));
        }

        [TestMethod]
        public async Task WhenUserTriesToAccessNonExistingConnection_ThenTheyReceiveNullResponse()
        {
            var organisationId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();

            await using var context = _context;
            await DatabaseDataGenerator.InsertRandomEnrolment(_context, organisationId, "Packaging.ApprovedPerson", DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var result = await _connectionsService.GetConnectionWithPersonForServiceAsync(connectionId, organisationId, "Packaging");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task WhenUserTriesToAccessConnectionFromDifferentOrganisation_ThenTheyReceiveNullResponse()
        {
            await using var context = _context;
            
            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(_context, organisationId,  "Packaging.ApprovedPerson", DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

            Guid otherOrganisationId = Guid.NewGuid();

            var result = await _connectionsService.GetConnectionWithPersonForServiceAsync(connectionId, otherOrganisationId, "Packaging");

            result.Should().BeNull();
        }

    }
}
