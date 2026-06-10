using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
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
    [Collection(SharedSqlCollection.Name)]
    [Trait("Category", "IntegrationTest")]
    public class DelegatedPersonEnrolmentsControllerTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
    {
        private readonly PerClassDbFixture _db;
        private AccountsDbContext _context = null!;
        private readonly Mock<IRoleManagementService> _roleManagementServiceMock = new();
        private DelegatedPersonEnrolmentsController _delegatedPersonEnrolmentController = null!;
        private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
        private const string BaseProblemTypePath = "https://epr-errors/";
        private static readonly NullLogger<DelegatedPersonEnrolmentsController> NullLogger = new();
        private readonly Guid _enrolmentId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _organisationId = Guid.NewGuid();
        private readonly string _serviceKey = "Packaging";

        public DelegatedPersonEnrolmentsControllerTests(PerClassDbFixture db)
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

            await _context.Database.MigrateAsync(default);

            _apiConfigOptionsMock
                .Setup(x => x.Value)
                .Returns(new ApiConfig
                {
                    BaseProblemTypePath = BaseProblemTypePath
                });

            _delegatedPersonEnrolmentController = new DelegatedPersonEnrolmentsController(
                _roleManagementServiceMock.Object,
                _apiConfigOptionsMock.Object,
                NullLogger);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        [Trait("Category", "AcceptNominationToDelegatedPerson")]
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

        [Fact]
        [Trait("Category", "AcceptNominationToDelegatedPerson")]
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

        [Fact]
        [Trait("Category", "AcceptNominationToDelegatedPerson")]
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

        [Fact]
        [Trait("Category", "AcceptNominationToDelegatedPerson")]
        public async Task AcceptNominationToDelegatedPerson_WhenEnrolmentIdIsAccepted_ShouldReturnCorrectResponse()
        {
            // Arrange
            var acceptNominationRequest = new Core.Models.Request.AcceptNominationRequest();
            _roleManagementServiceMock.Setup(x => x.AcceptNominationToDelegatedPerson(_enrolmentId, _userId, _organisationId, _serviceKey, acceptNominationRequest))
                .ReturnsAsync(new ValueTuple<bool, string>(true, string.Empty));

            // Act
            var result = await _delegatedPersonEnrolmentController.AcceptNominationToDelegatedPerson(_enrolmentId, _serviceKey, acceptNominationRequest, _userId, _organisationId);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsValid_ShouldReturnCorrectResponse()
        {
            // Arrange
            _roleManagementServiceMock.Setup(x => x.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey))
                .ReturnsAsync(new DelegatedPersonNominatorResponse());

            // Act
            var result = await _delegatedPersonEnrolmentController.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDelegatedPersonNominator_WhenEnrolmentIdIsNotValid_ShouldReturnErrorResponse()
        {
            // Arrange
            _roleManagementServiceMock.Setup(x => x.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey))
                .ReturnsAsync(Task.FromResult<DelegatedPersonNominatorResponse>(null).Result);

            // Act
            var result = await _delegatedPersonEnrolmentController.GetDelegatedPersonNominator(_enrolmentId, _userId, _organisationId, _serviceKey);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
