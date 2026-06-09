using System.Net;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class NotificationsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetNotifications_PackagingWithNominatedDelegatedPerson_Returns200WithNotification()
    {
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Nominated);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.GetAsync($"/api/notifications?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<NotificationsResponse>();
        payload.Notifications.Should().ContainSingle()
            .Which.Data.Should().Contain(kv => kv.Key == "EnrolmentId" && kv.Value == enrolment.ExternalId.ToString());
    }

    [Fact]
    public async Task GetNotifications_NoNotificationsForUser_Returns204()
    {
        // Nothing built: brand-new user/org guids, no enrolments to find.
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync($"/api/notifications?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetNotifications_PackagingApprovedPersonPending_Returns200WithNotification()
    {
        // The service includes Approved/Delegated persons in either Nominated *or* Pending status.
        // Builds a Pending ApprovedPerson and asserts the Pending notification type comes back.
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Pending);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.GetAsync($"/api/notifications?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<NotificationsResponse>();
        payload.Notifications.Should().ContainSingle()
            .Which.Data.Should().Contain(kv => kv.Key == "EnrolmentId" && kv.Value == enrolment.ExternalId.ToString());
    }

    [Fact]
    public async Task GetNotifications_NonPackagingService_Returns204EvenForKnownUser()
    {
        // The service only inspects Packaging enrolments — other service keys (ReprocessorExporter,
        // LaPayment) bypass the lookup and yield an empty notifications list → controller 204.
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Nominated);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.GetAsync($"/api/notifications?serviceKey={ServiceKeys.ReprocessorExporter}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetNotifications_MissingServiceKey_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNotifications_MissingUserHeader_Returns400()
    {
        // Use the raw Client without ClientFor — no X-EPR-User / X-EPR-Organisation headers.
        var response = await Client.GetAsync($"/api/notifications?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
