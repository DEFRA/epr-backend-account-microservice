using System.Net;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Features;

public class UserAndPersonLookupTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetSystemUserAndOrganisation_Returns200WithSystemIds()
    {
        // The "System" user + organisation are seeded by migrations and should always be present.
        var response = await Client.GetAsync("/api/users/system-user-and-organisation");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserOrganisation>();
        payload.UserId.Should().NotBe(Guid.Empty).And.NotBeNull();
        payload.OrganisationId.Should().NotBe(Guid.Empty).And.NotBeNull();
    }

    [Fact]
    public async Task GetUserIdByPersonId_WhenPersonExists_Returns200()
    {
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
        });

        var response = await Client.GetAsync($"/api/users/user-by-person-id?personId={enrolment.Connection.Person.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<Guid>();
        payload.Should().Be(enrolment.Connection.Person.User!.UserId!.Value);
    }

    [Fact]
    public async Task GetUserIdByPersonId_WhenPersonUnknown_Returns204()
    {
        var response = await Client.GetAsync($"/api/users/user-by-person-id?personId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetConnectionPerson_WithoutAuth_Returns403()
    {
        // Random user/org with no enrolment → IsAuthorisedToManageUsersFromOrganisationForService = false.
        var connectionId = Guid.NewGuid();
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync($"/api/connections/{connectionId}/person?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetConnectionPerson_WithAuthorisedAdminButUnknownConnection_Returns404()
    {
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var unknownConnectionId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/connections/{unknownConnectionId}/person?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConnectionPerson_ForBuiltConnection_Returns200WithPerson()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);

        var response = await client.GetAsync(
            $"/api/connections/{producer.Enrolment.Connection.ExternalId}/person?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The service projects Email from connection.Person.User.Email (not Person.Email).
        var payload = await response.ReadJson<ConnectionWithPersonResponse>();
        payload.Should().BeEquivalentTo(new
        {
            producer.Person.FirstName,
            producer.Person.LastName,
            Email = producer.Person.User!.Email,
            OrganisationName = producer.Organisation.Name,
            OrganisationReferenceNumber = producer.Organisation.ReferenceNumber,
        });
    }
}
