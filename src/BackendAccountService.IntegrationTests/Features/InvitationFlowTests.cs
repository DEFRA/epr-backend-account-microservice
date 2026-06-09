using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Features;

public class InvitationFlowTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task InviteUser_WithUnauthorisedInviter_Returns403()
    {
        var request = new AddInviteUserRequest
        {
            InvitedUser = new InvitedUser
            {
                Email = $"invitee-{Guid.NewGuid():N}@example.com",
                PersonRoleId = PersonRoleConst.Employee,
                ServiceRoleId = ServiceRoleConst.Packaging.BasicUser.Id,
                OrganisationId = Guid.NewGuid(),
            },
            InvitingUser = new InvitingUser
            {
                Email = "random@example.com",
                UserId = Guid.NewGuid(),
            },
        };

        var response = await Client.PostAsJsonAsync("/api/accounts-management/invite-user", request);

        // ExecuteProtectedAction => 403 when caller isn't an enrolled approved/admin person.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InviteUser_AsAuthorisedAdmin_Returns200WithInviteToken()
    {
        var inviter = await Builder.Producer().Build();

        var request = new AddInviteUserRequest
        {
            InvitedUser = new InvitedUser
            {
                Email = $"invitee-{Guid.NewGuid():N}@example.com",
                PersonRoleId = PersonRoleConst.Employee,
                ServiceRoleId = ServiceRoleConst.Packaging.BasicUser.Id,
                OrganisationId = inviter.OrgExternalId,
            },
            InvitingUser = new InvitingUser
            {
                Email = inviter.Person.Email,
                UserId = inviter.UserId,
            },
        };

        var response = await Client.PostAsJsonAsync("/api/accounts-management/invite-user", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var inviteToken = await response.Content.ReadAsStringAsync();
        inviteToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegulatorInviteUser_WithUnknownOrg_Returns400()
    {
        // Regulator path has no auth wall, just validation around the org/role matching the request.
        // A random OrganisationId fails the SingleAsync lookup inside the service → 400 (via the
        // middleware's InvalidOperationException handler).
        var request = new AddInviteUserRequest
        {
            InvitedUser = new InvitedUser
            {
                Email = $"reg-invitee-{Guid.NewGuid():N}@example.com",
                PersonRoleId = PersonRoleConst.Admin,
                ServiceRoleId = ServiceRoleConst.Regulator.Admin.Id,
                OrganisationId = Guid.NewGuid(),
            },
            InvitingUser = new InvitingUser
            {
                Email = "regulator-admin@example.com",
                UserId = Guid.NewGuid(),
            },
        };

        var response = await Client.PostAsJsonAsync("/api/regulator-accounts/invite-user", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegulatorInviteUser_WithBuiltRegulatorOrg_Returns200WithInviteToken()
    {
        // Point the request at the built regulator org so the service's SingleAsync(ExternalId ==
        // OrganisationId) lookup hits a real row.
        var regulator = await Builder.Regulator().Build();
        var request = new AddInviteUserRequest
        {
            InvitedUser = new InvitedUser
            {
                Email = $"reg-invitee-{Guid.NewGuid():N}@example.com",
                PersonRoleId = PersonRoleConst.Admin,
                ServiceRoleId = ServiceRoleConst.Regulator.Admin.Id,
                OrganisationId = regulator.OrgExternalId,
            },
            InvitingUser = new InvitingUser
            {
                Email = regulator.Person.Email,
                UserId = regulator.UserId,
            },
        };

        var response = await Client.PostAsJsonAsync("/api/regulator-accounts/invite-user", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var inviteToken = await response.Content.ReadAsStringAsync();
        inviteToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegulatorInvitedUser_GetByUnknownUserId_Returns200WithEmptyOrNull()
    {
        // The endpoint always returns OkObjectResult — for an unknown user it's an empty/null token,
        // not 404. Exercise the route + binding.
        var response = await Client.GetAsync($"/api/regulator-accounts/invited-user?userId={Guid.NewGuid()}&email=any@example.com");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadAsStringAsync()).Trim();
        // Endpoint serialises the missing token as JSON null or empty string depending on the
        // service's return shape. Either way it's "no token" — assert that explicitly.
        body.Should().BeOneOf("null", "\"\"", string.Empty);
    }

}
