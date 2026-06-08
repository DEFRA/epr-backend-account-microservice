using System.Net;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// OrganisationsController users endpoints: /users (IsAuthorisedToManageUsers-walled),
// /all-users, /team-members.
public class OrganisationUsersTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Users_NonAuthorisedUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/users?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&serviceRoleId=3");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Users_AsApprovedAdmin_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/organisations/users?userId={producer.UserId}&organisationId={producer.OrgExternalId}&serviceRoleId=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The service excludes the calling user from the list (e.Connection.Person.User.UserId != userId),
        // so a same-admin caller against their own org sees an empty list back. That still proves the
        // EF projection + serialisation wired up.
        var payload = await response.ReadJson<List<OrganisationUsersResponseModel>>();
        payload.Should().BeEmpty();
    }

    [Fact]
    public async Task AllUsers_AnyUser_Returns200()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/all-users?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&serviceRoleId=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<OrganisationUsersResponseModel>>();
        payload.Should().BeEmpty();
    }

    [Fact]
    public async Task TeamMembers_AnyUser_Returns200()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/team-members?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&serviceRoleId=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<TeamMembersResponseModel>>();
        payload.Should().BeEmpty();
    }
}
