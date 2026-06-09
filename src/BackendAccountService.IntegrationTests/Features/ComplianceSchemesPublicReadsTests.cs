using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class ComplianceSchemesPublicReadsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllComplianceSchemes_DeserialisesAsList()
    {
        var response = await Client.GetAsync("/api/compliance-schemes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Service joins ComplianceSchemes with Organisations where IsComplianceScheme=true on
        // CompaniesHouseNumber. Vanilla migrations include scheme rows but no operator-org rows,
        // so the list is empty — the assertion proves routing + EF projection wire up.
        var payload = await response.ReadJson<List<ComplianceSchemeDto>>();
        payload.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllComplianceSchemes_WithBuiltOperator_ContainsThatScheme()
    {
        var scheme = await Builder.SchemeOperator().Build();

        var response = await Client.GetAsync("/api/compliance-schemes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<ComplianceSchemeDto>>();
        payload.Should().Contain(cs => cs.Id == scheme.SchemeExternalId);
    }

    [Fact]
    public async Task GetMemberRemovalReasons_ReturnsSeedReasons()
    {
        var response = await Client.GetAsync("/api/compliance-schemes/member-removal-reasons");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<ComplianceSchemeRemovalReasonResponse>>();
        payload.Should().NotBeEmpty();
        payload.Should().AllSatisfy(r => r.Code.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task GetSchemesForOperator_UnknownOrg_Returns400()
    {
        var response = await Client.GetAsync(
            $"/api/compliance-schemes/get-for-operator?organisationId={Guid.NewGuid()}");

        // Service SingleOrDefaultAsync → null, then dereferences organisation.CompaniesHouseNumber,
        // throws NullReferenceException, lands on the wire as 400 (advertised 200/400 — the cause
        // is a missing null-check, not a request-validation failure).
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSchemesForOperator_WithBuiltOperator_Returns200WithMatchingScheme()
    {
        var scheme = await Builder.SchemeOperator().Build();

        var response = await Client.GetAsync(
            $"/api/compliance-schemes/get-for-operator?organisationId={scheme.OperatorExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<ComplianceSchemeDto>>();
        payload.Should().Contain(cs => cs.Id == scheme.SchemeExternalId);
    }

    [Fact]
    public async Task GetSchemeForProducer_UnknownOrg_Returns204()
    {
        var response = await Client.GetAsync(
            $"/api/compliance-schemes/get-for-producer?userOid={Guid.NewGuid()}&organisationId={Guid.NewGuid()}");

        // Service returns Result.FailedResult(..., HttpStatusCode.NoContent) when the org has no
        // selected scheme; BuildErrorResponse maps that straight to 204.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetSchemeForProducer_WithExistingSelection_Returns200()
    {
        // Directly build a SelectedScheme for the producer — bypasses the /select endpoint's
        // non-deterministic operator pick.
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var producer = await Builder.Producer().Build();
        await Builder.SchemeMembership(producer, operatorAdmin).Build();

        var response = await Client.GetAsync(
            $"/api/compliance-schemes/get-for-producer?userOid={producer.UserId}&organisationId={producer.OrgExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ProducerComplianceSchemeDto>();
        payload.ComplianceSchemeName.Should().Be(operatorAdmin.Scheme.Name);
        payload.ComplianceSchemeOperatorId.Should().Be(operatorAdmin.OperatorOrgExternalId);
    }

    [Fact]
    public async Task ExportComplianceSchemeSubsidiaries_UnknownOrg_Returns204()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync(
            $"/api/compliance-schemes/{Guid.NewGuid()}/schemes/{Guid.NewGuid()}/export-subsidiaries");

        // Service returns null for an unknown org → controller returns 204.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExportComplianceSchemeSubsidiaries_WithProducerAndSubsidiary_Returns200()
    {
        // Operator + producer SelectedScheme + a producer→child OrganisationRelationship row.
        // Route's organisationId is the OPERATOR externalId (service filters on
        // oto.ExternalId == organisationId).
        var operatorAdmin = await Builder.SchemeOperator().WithAdmin().Build();
        var producer = await Builder.Producer().Build();
        await Builder.SchemeMembership(producer, operatorAdmin).Build();
        await Builder.RelationshipFor(producer).Build();

        var client = ClientFor(operatorAdmin.UserId, operatorAdmin.OperatorOrgExternalId);
        var response = await client.GetAsync(
            $"/api/compliance-schemes/{operatorAdmin.OperatorOrgExternalId}/schemes/{operatorAdmin.SchemeExternalId}/export-subsidiaries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<ExportOrganisationSubsidiariesResponseModel>>();
        payload.Should().NotBeEmpty();
    }

}
