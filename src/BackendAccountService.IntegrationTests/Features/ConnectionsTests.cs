using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Features;

public class ConnectionsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetConnectionRoles_WithoutAuth_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync($"/api/connections/{Guid.NewGuid()}/roles?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetConnectionRoles_WithAuthButUnknownConnection_Returns404()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);

        var response = await client.GetAsync($"/api/connections/{Guid.NewGuid()}/roles?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConnectionRoles_WithAuthAndKnownConnection_Returns200()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);

        var response = await client.GetAsync($"/api/connections/{producer.Enrolment.Connection.ExternalId}/roles?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ConnectionWithEnrolmentsResponse>();
        payload.UserId.Should().Be(producer.UserId);
        payload.Enrolments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdatePersonRole_WithNonPackagingService_Returns404()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/connections/{Guid.NewGuid()}/roles?serviceKey=NotPackaging",
            new UpdatePersonRoleRequest { PersonRole = PersonRole.Employee });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonRole_WithoutAuth_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/connections/{Guid.NewGuid()}/roles?serviceKey={ServiceKeys.Packaging}",
            new UpdatePersonRoleRequest { PersonRole = PersonRole.Employee });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NominateDelegatedPerson_WithNonPackagingService_Returns404()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/connections/{Guid.NewGuid()}/delegated-person-nomination?serviceKey=NotPackaging",
            new DelegatedPersonNominationRequest { RelationshipType = RelationshipType.Employment });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NominateDelegatedPerson_WithoutAuth_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/connections/{Guid.NewGuid()}/delegated-person-nomination?serviceKey={ServiceKeys.Packaging}",
            new DelegatedPersonNominationRequest { RelationshipType = RelationshipType.Employment });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NominateDelegatedPerson_AsApprovedAdminAgainstSecondConnection_Returns200()
    {
        // Caller is an ApprovedPerson admin; target is a second BasicUser. Nomination requires the
        // target to be enrolled, not Invited, not already ApprovedPerson — BasicUser fits.
        var caller = await Builder.Producer().Build();
        Enrolment target = null!;
        await WithDbContext(async ctx =>
        {
            target = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, caller.OrgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Employee,
                EnrolmentStatusConst.Approved);
        });

        var client = ClientFor(caller.UserId, caller.OrgExternalId);
        var response = await client.PutAsJsonAsync(
            $"/api/connections/{target.Connection.ExternalId}/delegated-person-nomination?serviceKey={ServiceKeys.Packaging}",
            new DelegatedPersonNominationRequest
            {
                RelationshipType = RelationshipType.Employment,
                JobTitle = "Compliance Officer",
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePersonRole_AsApprovedAdminAgainstSecondConnection_Returns200()
    {
        // Caller is an ApprovedPerson admin; target is a second BasicUser in the same org. The
        // service rejects updating one's own connection and rejects updating an ApprovedPerson —
        // a BasicUser sibling sidesteps both guards.
        var caller = await Builder.Producer().Build();
        Enrolment target = null!;
        await WithDbContext(async ctx =>
        {
            target = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, caller.OrgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Employee,
                EnrolmentStatusConst.Approved);
        });

        var client = ClientFor(caller.UserId, caller.OrgExternalId);
        var response = await client.PutAsJsonAsync(
            $"/api/connections/{target.Connection.ExternalId}/roles?serviceKey={ServiceKeys.Packaging}",
            new UpdatePersonRoleRequest { PersonRole = PersonRole.Admin });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UpdatePersonRoleResponse>();
        // BasicUser → Admin role swap doesn't remove a service-role (only delegated-person path does).
        payload.RemovedServiceRoles.Should().BeNullOrEmpty();
    }
}
