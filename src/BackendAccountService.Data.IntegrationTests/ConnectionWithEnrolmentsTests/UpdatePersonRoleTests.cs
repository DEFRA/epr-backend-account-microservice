using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using BackendAccountService.Core.Models.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests
{
    [TestClass]
    public class UpdatePersonRoleTests
    {
        private static AzureSqlDbContainer _database = null!;
        private static DbContextOptions<AccountsDbContext> _options = null!;
        private AccountsDbContext _writeContext = null!;
        private RoleManagementService _connectionsService = null!;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext _)
        {
            _database = await AzureSqlDbContainer.StartDockerDbAsync();
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static async Task TestFixtureTearDown()
        {
            await _database.StopAsync();
        }

        [TestInitialize]
        public async Task Setup()
        {
            _options = new DbContextOptionsBuilder<AccountsDbContext>()
                    .UseSqlServer(_database.ConnectionString)
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .Options;

            _writeContext = new AccountsDbContext(_options);

            await _writeContext.Database.EnsureCreatedAsync();

            _connectionsService = new RoleManagementService(_writeContext, 
                new ValidationService(_writeContext, NullLogger<ValidationService>.Instance));
        }

        [TestMethod]
        public async Task WhenAdminIsUpdatedToEmployeePersonRole_ThenUpdateIsSuccessful()
        {
            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var adminEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
                (_writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            adminEnrolment.Connection.PersonRoleId.Should().Be(DbConstants.PersonRole.Admin);

            var result = await _connectionsService.UpdatePersonRoleAsync(
                connectionId: adminEnrolment.Connection.ExternalId,
                userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                personRole: PersonRole.Employee);

            result.Should().NotBeNull();

            var updatedEnrolment = await _writeContext.Enrolments.Where(enrolment => enrolment.Id == adminEnrolment.Id).FirstOrDefaultAsync();

            updatedEnrolment.Connection.PersonRoleId.Should().Be(DbConstants.PersonRole.Employee);
        }

        [TestMethod]
        public async Task WhenEmployeeIsUpdatedToAdminPersonRole_ThenUpdateIsSuccessful()
        {
            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var employeeEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Employee, DbConstants.EnrolmentStatus.Approved);

            var result = await _connectionsService.UpdatePersonRoleAsync(
                connectionId: employeeEnrolment.Connection.ExternalId,
                userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                personRole: PersonRole.Admin);

            result.Should().NotBeNull();

            var updatedEnrolment = await _writeContext.Enrolments.Where(enrolment => enrolment.Id == employeeEnrolment.Id).FirstOrDefaultAsync();

            updatedEnrolment.Connection.PersonRoleId.Should().Be(DbConstants.PersonRole.Admin);
        }

        [TestMethod]
        [DataRow(DbConstants.PersonRole.Employee, PersonRole.Employee)]
        [DataRow(DbConstants.PersonRole.Admin, PersonRole.Admin)]
        public async Task WhenDelegatedPersonIsDemotedToBasicUser_ThenThenNewEnrolmentIsIssued(int personRoleId, PersonRole personRole)
        {
            Guid organisationId = Guid.NewGuid();

            var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var delegatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var result = await _connectionsService.UpdatePersonRoleAsync(
                connectionId: delegatedPersonEnrolment.Connection.ExternalId,
                userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                personRole: personRole);

            result.Should().NotBeNull();
            result.RemovedServiceRoles.Should().HaveCount(1);
            result.RemovedServiceRoles.First().ServiceRoleKey.Should().Be("Packaging.DelegatedPerson");
            result.RemovedServiceRoles.First().EnrolmentStatus.Should().Be("Approved");

            var delegatedPersonEnrolmentOld = await _writeContext.Enrolments.Where(enrolment => enrolment.Id == delegatedPersonEnrolment.Id).FirstOrDefaultAsync();

            delegatedPersonEnrolmentOld.Should().BeNull();

            var delegatedPersonEnrolmentNew = await _writeContext
                .Enrolments
                .Include(enrolment => enrolment.ServiceRole)
                .Where(enrolment => enrolment.ConnectionId == delegatedPersonEnrolment.ConnectionId).FirstOrDefaultAsync();

            delegatedPersonEnrolmentNew.Id.Should().NotBe(delegatedPersonEnrolment.Id);
            delegatedPersonEnrolmentNew.ServiceRole.Key.Should().Be(DbConstants.ServiceRole.Packaging.BasicUser.Key);
            delegatedPersonEnrolmentNew.Connection.PersonRoleId.Should().Be(personRoleId);
            delegatedPersonEnrolment.IsDeleted.Should().Be(true);
            delegatedPersonEnrolmentNew.IsDeleted.Should().Be(false);
        }

        [TestMethod]
        [DataRow(DbConstants.PersonRole.Employee, PersonRole.Employee)]
        [DataRow(DbConstants.PersonRole.Admin, PersonRole.Admin)]
        public async Task WhenTheUpdateIsToAlreadyPresentRole_ThenLastUpdatedOnDoesNotChange(int personRoleId, PersonRole personRole)
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var basicUserEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRoleId, DbConstants.EnrolmentStatus.Approved);

            var result = await _connectionsService.UpdatePersonRoleAsync(
                connectionId: basicUserEnrolment.Connection.ExternalId,
                userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                personRole: personRole);

            result.Should().NotBeNull();

            var basicUserEnrolmentAfterUpdate = await _writeContext.Enrolments.Where(enrolment => enrolment.Id == basicUserEnrolment.Id).FirstOrDefaultAsync();

            basicUserEnrolmentAfterUpdate.Connection.LastUpdatedOn.Should().Be(basicUserEnrolment.Connection.LastUpdatedOn);
            basicUserEnrolmentAfterUpdate.LastUpdatedOn.Should().Be(basicUserEnrolment.LastUpdatedOn);
        }

        [TestMethod]
        public async Task WhenIncorrectServiceIsBeingUpdated_ThenFail()
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Func<Task> update = () =>
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: organisationId,
                    serviceKey: "Other Service - Not Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Unsupported service 'Other Service - Not Packaging'");
        }

        [TestMethod]
        public async Task WhenConnectionFromDifferentOrganisationIsUsed_ThenFail()
        {
            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
                (_writeContext, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Func<Task> update = () =>
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: updaterEnrolment.Connection.Organisation.ExternalId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("There is no matching record to update");
        }

        [TestMethod]
        public async Task WhenNotEnrolledUserIsEdited_ThenFail()
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
                (_writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedConnection = _writeContext.PersonOrganisationConnections.Add(new Entities.PersonOrganisationConnection
            {
                OrganisationId = updaterEnrolment.Connection.OrganisationId,
                PersonRoleId = DbConstants.PersonRole.Admin,
                Person = new Entities.Person
                {
                    Email = $"Email",
                    FirstName = $"FirstName",
                    LastName = $"LastName",
                    Telephone = $"TelephoneNumber",
                    User = new Entities.User
                    {
                        Email = $"Email",
                        UserId = Guid.NewGuid()
                    }
                }
            });

            _writeContext.SaveChanges(updaterEnrolment.Connection.Person.User.UserId.Value, organisationId);

            Func<Task> update = () =>
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedConnection.Entity.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: organisationId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Not enrolled user cannot be edited");
        }

        [TestMethod]
        public async Task WhenInvitedPersonIsEdited_ThenFail()
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
                (_writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Invited);

            Func<Task> update = () =>
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: updaterEnrolment.Connection.Organisation.ExternalId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Invited user cannot be edited");
        }

         [TestMethod]
        public async Task WhenApprovedPersonsPersonRoleIsEdited_ThenFail()
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Func<Task> update = () =>
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: organisationId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Approved person cannot be edited");
        }

        [TestMethod]
        [DataRow(DbConstants.ServiceRole.Packaging.BasicUser.Key)]
        [DataRow(DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)]
        public async Task WhenDelegatedPersonIsEditedByNonApprovedPerson_ThenFail(string nonApprovedPersonServiceKey)
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, nonApprovedPersonServiceKey, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Func<Task> update = () => 
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: editedEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: organisationId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Only approved person can edit delegated person enrolment");
        }

        [TestMethod]
        public async Task WhenOwnEnrolmentIsEdited_ThenFail()
        {
            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            Func<Task> update = () => 
                _connectionsService.UpdatePersonRoleAsync(
                    connectionId: updaterEnrolment.Connection.ExternalId,
                    userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                    organisationId: organisationId,
                    serviceKey: "Packaging",
                    personRole: PersonRole.Employee);

            await update.Should().ThrowAsync<RoleManagementException>().WithMessage("Updating own record is not permitted");
        }

        [TestMethod]
        [TestCategory("Nominating Delegated Person")]
        public async Task WhenDelegatedPersonIsDemoted_ThenDelegatedPersonEnrolmentGetsDeleted()
        {
            using var readContext = new AccountsDbContext(_options);

            Guid organisationId = Guid.NewGuid();

            var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

            var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _writeContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

            var nominateResult = await _connectionsService.NominateToDelegatedPerson(
                connectionId: editedEnrolment.Connection.ExternalId,
                userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
                {
                    RelationshipType = Entities.Conversions.RelationshipType.Consultancy,
                    ConsultancyName = "Test Consultancy",
                    NominatorDeclaration = "Test Tester"
                });

            nominateResult.Succeeded.Should().BeTrue();

            var delegatedPersonEnrolment = readContext.DelegatedPersonEnrolments
                .Where(enrolment =>
                    enrolment.Enrolment.Connection.Id == editedEnrolment.Connection.Id &&
                    enrolment.Enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
                .FirstOrDefault();

            delegatedPersonEnrolment.Should().NotBeNull();

            await _connectionsService.UpdatePersonRoleAsync(
                connectionId: editedEnrolment.Connection.ExternalId,
                userId: updaterEnrolment.Connection.Person.User.UserId.Value,
                organisationId: organisationId,
                serviceKey: "Packaging",
                personRole: PersonRole.Employee);

            delegatedPersonEnrolment = readContext.DelegatedPersonEnrolments
                .Where(enrolment =>
                    enrolment.Enrolment.Connection.Id == editedEnrolment.Connection.Id &&
                    enrolment.Enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
                .FirstOrDefault();

            delegatedPersonEnrolment.Should().BeNull();
        }
    }
}
