using System.Diagnostics;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAccountService.Data.IntegrationTests.Controllers.ConnectionControllerTests.ConnectionRolesTests;

[TestClass]
public class NominateToDelegatedPersonTests
{
    private static AzureSqlDbContainer _database = null!;
    private AccountsDbContext _context = null!;
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
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(_context, organisationId,  "Packaging.ApprovedPerson", DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        Guid connectionId = authorisedPersonEnrolment.Connection.ExternalId;

        Guid otherOrganisationId = Guid.NewGuid();

        var result = await _connectionsService.GetConnectionWithPersonForServiceAsync(connectionId, otherOrganisationId, "Packaging");

        result.Should().BeNull();
    }
}