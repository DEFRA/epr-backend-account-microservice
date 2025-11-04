using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using System.Net;

namespace BackendAccountService.Data.IntegrationTests.Controllers
{
    [TestClass]
    public class DelegatedPersonEnrolmentsControllerTests
    {
        private static AzureSqlDbContainer _database = null!;

        private static AccountsDbContext _context = null!;

        private static readonly Mock<IRoleManagementService> RoleManagementServiceMock = new();
        private static DelegatedPersonEnrolmentsController _delegatedPersonEnrolmentController = null!;
        private static readonly Mock<IOptions<ApiConfig>> ApiConfigOptionsMock = new();
        private const string BaseProblemTypePath = "https://epr-errors/";
        private static readonly NullLogger<DelegatedPersonEnrolmentsController> NullLogger = new();
        private readonly Guid _enrolmentId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _organisationId = Guid.NewGuid();
        private readonly string _serviceKey = "Packaging";

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext _)
        {
            _database = await AzureSqlDbContainer.StartDockerDbAsync(default);

            _context = new AccountsDbContext(
                new DbContextOptionsBuilder<AccountsDbContext>()
                    .UseSqlServer(_database.ConnectionString)
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .Options);

            await _context.Database.MigrateAsync(default);

            ApiConfigOptionsMock
                .Setup(x => x.Value)
                .Returns(new ApiConfig
                {
                    BaseProblemTypePath = BaseProblemTypePath
                });

            _delegatedPersonEnrolmentController = new DelegatedPersonEnrolmentsController(
                RoleManagementServiceMock.Object,
                ApiConfigOptionsMock.Object,
                NullLogger);
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static async Task TestFixtureTearDown()
        {
            await _database.StopAsync();
        }

        [TestMethod]
        [TestCategory("AcceptNominationToDelegatedPerson")]
        public async Task AcceptNominationToDelegatedPerson_WhenServiceKeyNotPackaging_ThenAcceptingNominationFails()
        {
            var nominatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Nominated);

            var result = await _delegatedPersonEnrolmentController.AcceptNominationToDelegatedPerson(
                enrolmentId: nominatedPersonEnrolment.ExternalId,
                serviceKey: "SomethingOtherThanPackaging",
                userId: nominatedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: Guid.NewGuid(),
                acceptNominationRequest: new Core.Models.Request.AcceptNominationRequest()) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            var problem = result.Value as ProblemDetails;

            problem.Should().NotBeNull();
            problem.Type.Should().Be("https://epr-errors/service-not-supported");
            problem.Status.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        [TestCategory("AcceptNominationToDelegatedPerson")]
        public async Task AcceptNominationToDelegatedPerson_WhenUserDoesNotMatchEnrolmentId_ThenAcceptingNominationFails()
        {
            var nominatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Nominated);

            var result = await _delegatedPersonEnrolmentController.AcceptNominationToDelegatedPerson(
                enrolmentId: nominatedPersonEnrolment.ExternalId,
                serviceKey: "Packaging",
                userId: Guid.NewGuid(),
                organisationId: nominatedPersonEnrolment.Connection.Organisation.ExternalId,
                acceptNominationRequest: new Core.Models.Request.AcceptNominationRequest()) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var problem = result.Value as ProblemDetails;

            problem.Should().NotBeNull();
            problem.Type.Should().Be("https://epr-errors/delegated-person-nomination");
            problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        [TestCategory("AcceptNominationToDelegatedPerson")]
        public async Task AcceptNominationToDelegatedPerson_WhenOrganisationDoesNotMatchEnrolmentId_ThenAcceptingNominationFails()
        {
            var nominatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Nominated);

            var result = await _delegatedPersonEnrolmentController.AcceptNominationToDelegatedPerson(
                enrolmentId: nominatedPersonEnrolment.ExternalId,
                serviceKey: "Packaging",
                userId: nominatedPersonEnrolment.Connection.Person.User.UserId.Value,
                organisationId: Guid.NewGuid(),
                acceptNominationRequest: new Core.Models.Request.AcceptNominationRequest()) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var problem = result.Value as ProblemDetails;

            problem.Should().NotBeNull();
            problem.Type.Should().Be("https://epr-errors/delegated-person-nomination");
            problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        [TestCategory("AcceptNominationToDelegatedPerson")]
        public async Task AcceptNominationToDelegatedPerson_WhenEnrolmentIdIsAccepted_ShouldReturnCorrectResponse()
        {
            // Arrange
            var acceptNominationRequest = new Core.Models.Request.AcceptNominationRequest();
            RoleManagementServiceMock.Setup(x => x.AcceptNominationToDelegatedPerson(_enrolmentId, _userId, _organisationId, _serviceKey, acceptNominationRequest))
                .ReturnsAsync(new ValueTuple<bool, string>(true, string.Empty));

            // Act
            var result = await _delegatedPersonEnrolmentController.AcceptNominationToDelegatedPerson(_enrolmentId, _serviceKey, acceptNominationRequest, _userId, _organisationId);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [TestMethod]
        public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsValid_ShouldReturnCorrectResponse()
        {
            // Arrange
            RoleManagementServiceMock.Setup(x => x.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey))
                .ReturnsAsync(new DelegatedPersonNominatorResponse());

            // Act
            var result = await _delegatedPersonEnrolmentController.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [TestMethod]
        public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsNotValid_ShouldReturnErrorResponse()
        {
            // Arrange
            RoleManagementServiceMock.Setup(x => x.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey))
                .ReturnsAsync(Task.FromResult<DelegatedPersonNominatorResponse>(null).Result);

            // Act
            var result = await _delegatedPersonEnrolmentController.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}