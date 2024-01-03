using System.Diagnostics;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BackendAccountService.Data.IntegrationTests.Controllers;

[TestClass]
public class ConnectionsControllerTests
{
    private static AzureSqlEdgeDbContainer _database = null!;

    private static AccountsDbContext _context = null!;

    private static ConnectionsController _controller = null!;
    
    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlEdgeDbContainer.StartDockerDbAsync();

        _context = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_database.ConnectionString)
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options);

        await _context.Database.EnsureCreatedAsync();

        Mock<IOptions<ApiConfig>> apiConfigOptionsMock = new();
        
        apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://epr-errors/"
            });

        _controller = new ConnectionsController(
            new ValidationService(_context, new Mock<ILogger<ValidationService>>().Object),
            new RoleManagementService(_context, 
                new ValidationService(_context, NullLogger<ValidationService>.Instance)),
            apiConfigOptionsMock.Object,
            new Mock<ILogger<ConnectionsController>>().Object);
    }

    [ClassCleanup]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    public async Task WhenUserOfOneServiceIsAccessingNotRelatedServiceWithinTheSameOrganisation_ThenReturnedEnrolmentsAreEmpty()
    {
        Guid organisationId = Guid.NewGuid();
            
        var approvedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var otherService = _context.Services.Add(new Service
        {
            Key = "OtherService",
            Name = "Other Service"
        });

        await _context.SaveChangesAsync(approvedPersonEnrolment.Connection.Person.User.UserId.Value, organisationId);

        _context.ServiceRoles.Add(new ServiceRole()
        {
            Key = "OtherService.ApprovedPerson",
            Name = "Other Service Approved Person",
            ServiceId = otherService.Entity.Id
        });
        
        await _context.SaveChangesAsync(approvedPersonEnrolment.Connection.Person.User.UserId.Value, organisationId);
        
        var otherServicePersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, "OtherService.ApprovedPerson", DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        otherServicePersonEnrolment.Connection.OrganisationId.Should().Be(approvedPersonEnrolment.Connection.OrganisationId);
        otherServicePersonEnrolment.Connection.Organisation.ExternalId.Should().Be(approvedPersonEnrolment.Connection.Organisation.ExternalId);
        
        otherServicePersonEnrolment.ConnectionId.Should().NotBe(approvedPersonEnrolment.ConnectionId);

        var result = await _controller.GetConnectionAndEnrolments(
            otherServicePersonEnrolment.Connection.ExternalId, 
            "Packaging",
            approvedPersonEnrolment.Connection.Person.User.UserId.Value, 
            organisationId);

        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var connectionWithEnrolment = okResponse.Value as ConnectionWithEnrolmentsResponse;
        connectionWithEnrolment.PersonRole.Should().Be(PersonRole.Admin);
        connectionWithEnrolment.Enrolments.Should().BeEmpty();
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    public async Task UpdatePersonRole_WhenNonAdminUpdatesPersonRole_ThenReturnAuthorisationProblem403()
    {
        Guid organisationId = Guid.NewGuid();

        var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Employee, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var result = await _controller.UpdatePersonRole(
            connectionId: editedEnrolment.Connection.ExternalId,
            updateRequest: new Core.Models.Request.UpdatePersonRoleRequest { PersonRole = PersonRole.Admin },
            serviceKey: "Packaging",
            userId: updaterEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    public async Task UpdatePersonRole_WhenUserFromDifferentOrganisationUpdatesPersonRole_ThenReturnAuthorisationProblem403()
    {
        var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var result = await _controller.UpdatePersonRole(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Packaging",
            updateRequest: new Core.Models.Request.UpdatePersonRoleRequest { PersonRole = PersonRole.Admin },
            userId: updaterEnrolment.Connection.Person.User.UserId.Value,
            organisationId: editedEnrolment.Connection.Organisation.ExternalId) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    [TestCategory("Nominating Delegated Person")]
    public async Task WhenApprovedPersonTriesToNominateWithinNonPackagingService_ThenNominationFails()
    {
        var nonApprovedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _controller.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Other than Packaging",
            userId: nonApprovedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: editedEnrolment.Connection.Organisation.ExternalId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/service-not-supported");
        problem.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenNonApprovedPersonTriesToNominate_ThenReturnAuthorisationProblem403()
    {
        Guid organisationId = Guid.NewGuid();

        var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _controller.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Packaging",
            userId: updaterEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenApprovedPersonFromDifferentOrganisationTriesToNominate_ThenReturnAuthorisationProblem403()
    {
        var updaterEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var result = await _controller.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Packaging",
            userId: updaterEnrolment.Connection.Person.User.UserId.Value,
            organisationId: editedEnrolment.Connection.Organisation.ExternalId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    [TestCategory("Nominating Delegated Person")]
    [DataRow(DbConstants.EnrolmentStatus.NotSet)]
    [DataRow(DbConstants.EnrolmentStatus.Enrolled)]
    [DataRow(DbConstants.EnrolmentStatus.Rejected)]
    [DataRow(DbConstants.EnrolmentStatus.Invited)]
    [DataRow(DbConstants.EnrolmentStatus.Nominated)]
    public async Task WhenNonApprovedPersonTriesToNominateEmployee_ThenNominationFails(int nonApprovedEnrolmentStatus)
    {
        var nonApprovedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, nonApprovedEnrolmentStatus);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Employee, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _controller.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Packaging",
            userId: nonApprovedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: editedEnrolment.Connection.Organisation.ExternalId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("ConnectionsController")]
    [TestCategory("Nominating Delegated Person")]
    [DataRow(DbConstants.EnrolmentStatus.NotSet)]
    [DataRow(DbConstants.EnrolmentStatus.Enrolled)]
    [DataRow(DbConstants.EnrolmentStatus.Rejected)]
    [DataRow(DbConstants.EnrolmentStatus.Invited)]
    [DataRow(DbConstants.EnrolmentStatus.Nominated)]
    public async Task WhenNonApprovedPersonTriesToNominateAdmin_ThenNominationFails(int nonApprovedEnrolmentStatus)
    {
        var nonApprovedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, nonApprovedEnrolmentStatus);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, Guid.NewGuid(), DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _controller.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            serviceKey: "Packaging",
            userId: nonApprovedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: editedEnrolment.Connection.Organisation.ExternalId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://epr-errors/authorisation");
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}