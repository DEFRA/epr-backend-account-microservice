using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [TestClass]
    public class IsAuthorisedToManageUsersFromOrganisationForServiceTests
    {
        private static AzureSqlDbContainer _database = null!;

        private AccountsDbContext _context = null!;
        private ValidationService _validationService = null!;

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

            _validationService = new ValidationService(_context, new Mock<ILogger<ValidationService>>().Object);
        }

        [TestMethod]
        public async Task WhenUserIsApprovedPerson_ThenTheyCanManageTheirOwnUsers()
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

            var serviceKey = "Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(
                approvedPersonEnrolment.Connection.Person.User.UserId.Value,
                approvedPersonEnrolment.Connection.Organisation.ExternalId,
                serviceKey);

            canManageOwnUsers.Should().BeTrue();
        }

        [TestMethod]
        public async Task WhenUserIsApprovedPerson_ThenTheyCannotManageUsersFromOtherOrganisations()
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

            var otherCompanyApprovedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

            var serviceKey = "Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(
                otherCompanyApprovedPersonEnrolment.Connection.Person.User.UserId.Value,
                approvedPersonEnrolment.Connection.Organisation.ExternalId,
                serviceKey);

            canManageOwnUsers.Should().BeFalse();
        }

        [TestMethod]
        public async Task WhenUserIsApprovedPerson_ThenTheyCannotManageUsersForOtherServices()
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

            var serviceKey = "Some Other Service - Not Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(
                approvedPersonEnrolment.Connection.Person.User.UserId.Value,
                approvedPersonEnrolment.Connection.Organisation.ExternalId,
                serviceKey);

            canManageOwnUsers.Should().BeFalse();
        }

        [DataRow(DbConstants.EnrolmentStatus.Enrolled)]
        [DataRow(DbConstants.EnrolmentStatus.Pending)]
        [DataRow(DbConstants.EnrolmentStatus.Approved)]
        [TestMethod]
        public async Task WhenEnrolmentStatusOfApprovedPersonIsAppropriate_ThenTheyCanManageTheirUsers(int authorisedToManageEnrolmentStatus)
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
            approvedPersonEnrolment.EnrolmentStatusId = authorisedToManageEnrolmentStatus;

            Guid userId = approvedPersonEnrolment.Connection.Person.User.UserId.Value;
            Guid organisationId = approvedPersonEnrolment.Connection.Organisation.ExternalId;
            await context.SaveChangesAsync(userId, organisationId);

            approvedPersonEnrolment.EnrolmentStatusId.Should().Be(authorisedToManageEnrolmentStatus);

            var serviceKey = "Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);

            canManageOwnUsers.Should().BeTrue();
        }

        [DataRow(DbConstants.EnrolmentStatus.NotSet)]
        [DataRow(DbConstants.EnrolmentStatus.Invited)]
        [DataRow(DbConstants.EnrolmentStatus.Rejected)]
        [TestMethod]
        public async Task WhenEnrolmentStatusOfApprovedPersonIsInappropriate_ThenTheyCannotManageTheirUsers(int authorisedToManageEnrolmentStatus)
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
            approvedPersonEnrolment.EnrolmentStatusId = authorisedToManageEnrolmentStatus;

            Guid userId = approvedPersonEnrolment.Connection.Person.User.UserId.Value;
            Guid organisationId = approvedPersonEnrolment.Connection.Organisation.ExternalId;
            await context.SaveChangesAsync(userId, organisationId);

            approvedPersonEnrolment.EnrolmentStatusId.Should().Be(authorisedToManageEnrolmentStatus);

            var serviceKey = "Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);

            canManageOwnUsers.Should().BeFalse();
        }


        private async Task<Enrolment> GenerateRandomPendingEnrolment(string serviceRoleKey)
        {
            var accountService = new AccountService(_context, new Mock<ITokenService>().Object, new Mock<IReExEnrolmentMaps>().Object);

            var approvedPersonAccount = RandomModelData.GetAccountModel(serviceRoleKey);
            var serviceRole = await accountService.GetServiceRoleAsync(approvedPersonAccount.Connection.ServiceRole);
            return await accountService.AddAccountAsync(approvedPersonAccount, serviceRole);
        }
    }
}
