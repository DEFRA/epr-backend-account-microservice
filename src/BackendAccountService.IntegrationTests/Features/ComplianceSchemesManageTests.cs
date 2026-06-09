using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// Endpoints guarded by ExecuteProtectedAction (IsAuthorisedToManageComplianceScheme).
// Random Guids never pass the auth check — 403 tests prove routing + auth wiring.
// Happy paths pair Builder.Producer() (ApprovedPerson admin — satisfies the auth check)
// with Builder.SchemeOperator() (operator org that backs a real ComplianceScheme row,
// required for the service's CompaniesHouseNumber join).
public class ComplianceSchemesManageTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RemoveComplianceScheme_NonAuthorisedUser_Returns403()
    {
        var request = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = Guid.NewGuid(),
            UserOId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/remove", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveComplianceScheme_ForExistingSelection_Returns200()
    {
        // Directly build the SelectedScheme row rather than going through /select — the production
        // endpoint picks the FIRST operator with matching CompaniesHouseNumber and is
        // non-deterministic when prior tests leave other operator orgs around with the same CH.
        var producer = await Builder.Producer().Build();
        var scheme = await Builder.SchemeOperator().Build();
        var selectedSchemeId = await Builder.SchemeMembership(producer, scheme).Build();

        var request = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = selectedSchemeId,
            UserOId = producer.UserId,
            OrganisationId = producer.OrgExternalId,
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/remove", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SelectComplianceScheme_NonAuthorisedUser_Returns403()
    {
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = Guid.NewGuid(),
            ProducerOrganisationId = Guid.NewGuid(),
            UserOId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/select", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SelectComplianceScheme_AsAuthorisedProducerAdmin_Returns200WithSelectedSchemeDto()
    {
        var producer = await Builder.Producer().Build();
        var scheme = await Builder.SchemeOperator().Build();

        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = scheme.SchemeExternalId,
            ProducerOrganisationId = producer.OrgExternalId,
            UserOId = producer.UserId,
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/select", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<SelectedSchemeDto>();
        payload.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task UpdateSelectedComplianceScheme_NonAuthorisedUser_Returns403()
    {
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = Guid.NewGuid(),
            ComplianceSchemeId = Guid.NewGuid(),
            ProducerOrganisationId = Guid.NewGuid(),
            UserOid = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/update", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateSelectedComplianceScheme_ForExistingSelection_Returns200WithSelectedSchemeDto()
    {
        // Re-selects the same scheme. UpdateSelectedComplianceSchemeAsync removes the existing
        // SelectedScheme row then re-adds via the same producer/scheme pair, so the payload's
        // ExternalId is a fresh value distinct from the original selection.
        var producer = await Builder.Producer().Build();
        var scheme = await Builder.SchemeOperator().Build();
        var selectedSchemeId = await Builder.SchemeMembership(producer, scheme).Build();

        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = selectedSchemeId,
            ComplianceSchemeId = scheme.SchemeExternalId,
            ProducerOrganisationId = producer.OrgExternalId,
            UserOid = producer.UserId,
        };

        var response = await Client.PostAsJsonAsync("/api/compliance-schemes/update", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<SelectedSchemeDto>();
        payload.Id.Should().NotBe(Guid.Empty).And.NotBe(selectedSchemeId);
    }
}
