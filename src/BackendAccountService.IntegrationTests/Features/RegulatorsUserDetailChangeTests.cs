using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController user-details change-request endpoints: per-org approval (POST, headers
// for auth), paged listing of pending requests, single-request read.
public class RegulatorsUserDetailChangeTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    // userId / orgId come from X-EPR-User / X-EPR-Organisation headers, not query/route.
    [Fact]
    public async Task AcceptOrRejectUserDetailsChangeRequest_NonRegulatorUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());
        var body = new ManageUserDetailsChangeRequest { HasRegulatorAccepted = true, RegulatorComment = null };

        var response = await client.PostAsJsonAsync(
            $"/api/regulators/regulator-organisation/approval/{Guid.NewGuid()}",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AcceptOrRejectUserDetailsChangeRequest_ForActiveChangeHistory_Returns200()
    {
        // Service requires an active ChangeHistory row (DecisionDate==null, IsActive, !IsDeleted)
        // keyed on the producer person/org, plus the caller resolving to a regulator.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();
        var changeHistoryExternalId = await Builder.ChangeHistoryFor(producer).Build();

        var client = ClientFor(regulator.UserId, regulator.OrgExternalId);
        var body = new ManageUserDetailsChangeRequest { HasRegulatorAccepted = true, RegulatorComment = null };

        var response = await client.PostAsJsonAsync(
            $"/api/regulators/regulator-organisation/approval/{changeHistoryExternalId}",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<RegulatorUserDetailsUpdateResponse>();
        payload.HasUserDetailsChangeAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingUserDetailChangeRequests_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/accounts/pending-user-change-requests?userId={Guid.NewGuid()}&currentPage=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingUserDetailChangeRequests_AsRegulator_Returns200()
    {
        // Paginated; the setup need only put the regulator in place — empty Items is a valid 200.
        var regulator = await Builder.Regulator().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/accounts/pending-user-change-requests?userId={regulator.UserId}&currentPage=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserDetailChangeRequestsPage>();
        payload.CurrentPage.Should().Be(1);
        payload.PageSize.Should().Be(10);
        payload.Items.Should().NotBeNull();
    }

    private sealed record UserDetailChangeRequestsPage(List<OrganisationUserDetailChangeRequest> Items, int CurrentPage, int TotalItems, int PageSize);

    [Fact]
    public async Task GetUserDetailChangeRequest_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/accounts/pending-user-change?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&externalId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserDetailChangeRequest_ForActiveChangeHistory_Returns200()
    {
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();
        var changeHistoryExternalId = await Builder.ChangeHistoryFor(producer).Build();

        var response = await Client.GetAsync(
            $"/api/regulators/accounts/pending-user-change?userId={regulator.UserId}&organisationId={producer.OrgExternalId}&externalId={changeHistoryExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ChangeHistoryModel>();
        payload.Should().NotBeNull();
    }
}
