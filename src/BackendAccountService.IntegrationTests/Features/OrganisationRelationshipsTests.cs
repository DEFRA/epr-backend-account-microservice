using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// OrganisationsController relationships endpoints: per-org relationships, paged/unpaged
// listings, and export-subsidiaries (which reads the same relationship rows).
public class OrganisationRelationshipsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetOrganisationRelationshipsByOrganisationId_Random_Returns204()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/{Guid.NewGuid()}/organisationRelationships");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetOrganisationRelationshipsByOrganisationId_WithExistingRelationship_Returns200()
    {
        // Builds a parent org (the Builder.Producer() org) + a separate child org + an active
        // OrganisationRelationship row pointing parent → child. The endpoint joins on
        // FirstOrganisationId == the parent and RelationToDate == null, so the row qualifies.
        var parent = await Builder.Producer().Build();
        await Builder.RelationshipFor(parent).Build();

        var response = await Client.GetAsync(
            $"/api/organisations/{parent.OrgExternalId}/organisationRelationships");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationRelationshipResponseModel>();
        payload.Organisation.Should().NotBeNull();
        payload.Relationships.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPagedOrganisationRelationships_WithParams_Returns200()
    {
        var response = await Client.GetAsync(
            "/api/organisations/organisationRelationships?page=1&showPerPage=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<PagedOrganisationRelationshipsResponse>();
        payload.CurrentPage.Should().Be(1);
        payload.PageSize.Should().Be(10);
        payload.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPagedOrganisationRelationships_MissingPage_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/organisations/organisationRelationships?showPerPage=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUnpagedOrganisationRelationships_Returns200()
    {
        var response = await Client.GetAsync("/api/organisations/organisationRelationshipsWithoutPaging");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<RelationshipResponseModel>>();
        payload.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportSubsidiaries_RandomOrg_Returns204()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/{Guid.NewGuid()}/export-subsidiaries");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExportSubsidiaries_WithExistingRelationship_Returns200WithList()
    {
        // Same setup shape as the relationships happy path — service requires at least one active
        // OrganisationRelationship FirstOrganisationId → SecondOrganisationId for the parent org.
        var parent = await Builder.Producer().Build();
        await Builder.RelationshipFor(parent).Build();

        var response = await Client.GetAsync(
            $"/api/organisations/{parent.OrgExternalId}/export-subsidiaries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<ExportOrganisationSubsidiariesResponseModel>>();
        payload.Should().NotBeEmpty();
    }
}
