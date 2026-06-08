using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// Endpoints guarded by ExecuteProtectedComplianceSchemeAction
// (IsAuthorisedToViewComplianceSchemeMembers — requires the user be enrolled on an
// Organisation where IsComplianceScheme=true). Random Guids never pass the auth check;
// 403 tests prove routing + auth wiring. Happy paths use Builder.SchemeOperator().WithAdmin().
public class ComplianceSchemesMemberTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetComplianceSchemeMembers_NonAuthorisedUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{Guid.NewGuid()}/schemes/{Guid.NewGuid()}/scheme-members?pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetComplianceSchemeMembers_AsOperatorAdmin_Returns200WithEmptyMembership()
    {
        // No producer has selected the scheme yet, so the service short-circuits with
        // LinkedOrganisationCount = 0 and a SuccessResult — proves auth + matching-operator wiring.
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{operatorAdmin.OperatorOrgExternalId}/schemes/{operatorAdmin.SchemeExternalId}/scheme-members?pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ComplianceSchemeMembershipResponse>();
        payload.SchemeName.Should().Be(operatorAdmin.Scheme.Name);
        payload.LinkedOrganisationCount.Should().Be(0);
    }

    [Fact]
    public async Task GetComplianceSchemeMemberDetails_NonAuthorisedUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{Guid.NewGuid()}/scheme-members/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetComplianceSchemeMemberDetails_ForProducerSelection_Returns200()
    {
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var producer = await Builder.Producer().Build();
        var selectedSchemeId = await Builder.SchemeMembership(producer, operatorAdmin).Build();
        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{operatorAdmin.OperatorOrgExternalId}/scheme-members/{selectedSchemeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ComplianceSchemeMemberDetailDto>();
        payload.ComplianceScheme.Should().Be(operatorAdmin.Scheme.Name);
    }

    [Fact]
    public async Task GetSchemeRemovalInfo_NonAuthorisedUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{Guid.NewGuid()}/scheme-members/{Guid.NewGuid()}/removal");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSchemeRemovalInfo_ForProducerSelection_Returns200()
    {
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var producer = await Builder.Producer().Build();
        var selectedSchemeId = await Builder.SchemeMembership(producer, operatorAdmin).Build();
        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{operatorAdmin.OperatorOrgExternalId}/scheme-members/{selectedSchemeId}/removal");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<InfoForSelectedSchemeRemoval>();
        payload.OrganisationName.Should().Be(producer.Organisation.Name);
    }

    [Fact]
    public async Task RemoveSchemeMember_NonAuthorisedUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());
        var body = new RemoveComplianceSchemeMemberRequest { Code = "OPERATOR_LEFT_SCHEME" };

        var response = await client.PostAsJsonAsync(
            $"/api/compliance-schemes/{Guid.NewGuid()}/scheme-members/{Guid.NewGuid()}/removed",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveSchemeMember_ForProducerSelection_Returns200()
    {
        // Code "A" is a non-reason-required option populated by ComplianceSchemeMemberRemovalReasonConfig.
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var producer = await Builder.Producer().Build();
        var selectedSchemeId = await Builder.SchemeMembership(producer, operatorAdmin).Build();
        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);
        var body = new RemoveComplianceSchemeMemberRequest { Code = "A" };

        var response = await client.PostAsJsonAsync(
            $"/api/compliance-schemes/{operatorAdmin.OperatorOrgExternalId}/scheme-members/{selectedSchemeId}/removed",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<RemoveComplianceSchemeMemberResponse>();
        payload.OrganisationName.Should().Be(producer.Organisation.Name);
    }

    // userId / orgId come from X-EPR-User / X-EPR-Organisation headers, not query/route.
    [Fact]
    public async Task GetComplianceSchemeSummary_NonAuthorisedUser_Returns403()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync($"/api/compliance-schemes/{Guid.NewGuid()}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetComplianceSchemeSummary_AsOperatorAdmin_Returns200()
    {
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{operatorAdmin.SchemeExternalId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ComplianceSchemeSummary>();
        payload.Name.Should().Be(operatorAdmin.Scheme.Name);
    }

}
