using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;

namespace BackendAccountService.Data.IntegrationTests.Controllers;

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
public class NotificationsControllerTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
{
    private readonly PerClassDbFixture _db;
    private AccountsDbContext _context = null!;
    private NotificationsController _controller = null!;

    public NotificationsControllerTests(PerClassDbFixture db)
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

        Mock<IOptions<ApiConfig>> apiConfigOptionsMock = new();

        apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://epr-errors/"
            });

        _controller = new NotificationsController(
            new NotificationsService(_context),
            apiConfigOptionsMock.Object);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    [Trait("Category", "NotificationsController")]
    public async Task GetNotifications_WhenDelegatedPersonPendingApprovalExists_ThenResultContainsCorrectNotification()
    {
        var organisationId = Guid.NewGuid();

        var pendingEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Pending);

         var actionResult = await _controller.GetNotifications("Packaging", pendingEnrolment.Connection.Person.User.UserId.Value, organisationId);

        actionResult.Should().NotBeNull();

        var result = actionResult.Result as OkObjectResult;
        result.Should().NotBeNull();

        var notificationsResponse = result.Value as NotificationsResponse;
        notificationsResponse.Should().NotBeNull();

        notificationsResponse.Notifications.Count.Should().Be(1);
        notificationsResponse.Notifications[0].Type.Should().Be(NotificationTypes.Packaging.DelegatedPersonPendingApproval);
    }


    [Fact]
    [Trait("Category", "NotificationsController")]
    public async Task GetNotifications_WhenDelegatedPersonNominationExists_ThenResultContainsCorrectNotification()
    {
        var organisationId = Guid.NewGuid();

        var pendingEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Nominated);

        var actionResult = await _controller.GetNotifications("Packaging", pendingEnrolment.Connection.Person.User.UserId.Value, organisationId);

        actionResult.Should().NotBeNull();

        var result = actionResult.Result as OkObjectResult;
        result.Should().NotBeNull();

        var notificationsResponse = result.Value as NotificationsResponse;
        notificationsResponse.Should().NotBeNull();

        notificationsResponse.Notifications.Count.Should().Be(1);
        notificationsResponse.Notifications[0].Type.Should().Be(NotificationTypes.Packaging.DelegatedPersonNomination);
    }

    [Fact]
    [Trait("Category", "NotificationsController")]
    public async Task GetNotifications_WhenNoNotificationExists_ThenResultIsNoContentResult()
    {
        var organisationId = Guid.NewGuid();

        var activeEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var actionResult = await _controller.GetNotifications("Packaging", activeEnrolment.Connection.Person.User.UserId.Value, organisationId);

        actionResult.Should().NotBeNull();

        var result = actionResult.Result as NoContentResult;
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "NotificationsController")]
    public async Task GetNotifications_WhenUserNotInOrganiation_ThenResultIsNotFoundResult()
    {
        var organisationId = Guid.NewGuid();
        var otherOrganisationId = Guid.NewGuid();

        var activeEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, otherOrganisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Pending);

        var actionResult = await _controller.GetNotifications("Packaging", activeEnrolment.Connection.Person.User.UserId.Value, otherOrganisationId);

        actionResult.Should().NotBeNull();

        var result = actionResult.Result as NoContentResult;
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "NotificationsController")]
    public async Task GetNotifications_WhenServiceNotPackaging_ThenResultIsNotFoundResult()
    {
        var organisationId = Guid.NewGuid();
        var otherOrganisationId = Guid.NewGuid();

        var activeEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _context, organisationId, DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Pending);

        var actionResult = await _controller.GetNotifications("DummyService", activeEnrolment.Connection.Person.User.UserId.Value, otherOrganisationId);

        actionResult.Should().NotBeNull();

        var result = actionResult.Result as NoContentResult;
        result.Should().NotBeNull();
    }

}
