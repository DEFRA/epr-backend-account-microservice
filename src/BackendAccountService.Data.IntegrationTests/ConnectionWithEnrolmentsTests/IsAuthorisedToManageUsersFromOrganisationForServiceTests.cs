using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [Collection(SharedSqlCollection.Name)]
    [Trait("Category", "IntegrationTest")]
    public class IsAuthorisedToManageUsersFromOrganisationForServiceTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
    {
        private readonly PerClassDbFixture _db;

        private AccountsDbContext _context = null!;
        private ValidationService _validationService = null!;

        public IsAuthorisedToManageUsersFromOrganisationForServiceTests(PerClassDbFixture db)
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

            _validationService = new ValidationService(_context, new Mock<ILogger<ValidationService>>().Object);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Theory]
        [InlineData(DbConstants.EnrolmentStatus.Enrolled)]
        [InlineData(DbConstants.EnrolmentStatus.Pending)]
        [InlineData(DbConstants.EnrolmentStatus.Approved)]
        public async Task WhenEnrolmentStatusOfApprovedPersonIsAppropriate_ThenTheyCanManageTheirUsers(int authorisedToManageEnrolmentStatus)
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
            approvedPersonEnrolment.EnrolmentStatusId = authorisedToManageEnrolmentStatus;

            Guid userId = approvedPersonEnrolment.Connection.Person.User.UserId.Value;
            Guid organisationId = approvedPersonEnrolment.Connection.Organisation.ExternalId;
            await context.SaveChangesAsync(userId, organisationId, default);

            approvedPersonEnrolment.EnrolmentStatusId.Should().Be(authorisedToManageEnrolmentStatus);

            var serviceKey = "Packaging";

            var canManageOwnUsers = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);

            canManageOwnUsers.Should().BeTrue();
        }

        [Theory]
        [InlineData(DbConstants.EnrolmentStatus.NotSet)]
        [InlineData(DbConstants.EnrolmentStatus.Invited)]
        [InlineData(DbConstants.EnrolmentStatus.Rejected)]
        public async Task WhenEnrolmentStatusOfApprovedPersonIsInappropriate_ThenTheyCannotManageTheirUsers(int authorisedToManageEnrolmentStatus)
        {
            await using var context = _context;

            var approvedPersonEnrolment = await GenerateRandomPendingEnrolment(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);
            approvedPersonEnrolment.EnrolmentStatusId = authorisedToManageEnrolmentStatus;

            Guid userId = approvedPersonEnrolment.Connection.Person.User.UserId.Value;
            Guid organisationId = approvedPersonEnrolment.Connection.Organisation.ExternalId;
            await context.SaveChangesAsync(userId, organisationId, default);

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
