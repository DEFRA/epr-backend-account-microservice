using System.Diagnostics;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAccountService.Data.IntegrationTests.Controllers.ConnectionControllerTests.ConnectionRolesTests;

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class NominateToDelegatedPersonTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
{
    private readonly PerClassDbFixture _db;
    private AccountsDbContext _context = null!;
    private RoleManagementService _connectionsService = null!;

    public NominateToDelegatedPersonTests(PerClassDbFixture db)
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

        _connectionsService = new RoleManagementService(_context, new ValidationService(_context, NullLogger<ValidationService>.Instance));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
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
