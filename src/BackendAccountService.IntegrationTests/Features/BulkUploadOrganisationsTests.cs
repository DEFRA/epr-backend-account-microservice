using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class BulkUploadOrganisationsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetOrganisationsByCompaniesHouseNumber_Returns200()
    {
        var chNumber = $"BCH-{Guid.NewGuid():N}".Substring(0, 12);
        await Builder.Organisation("Bulk CH Co").WithCompaniesHouseNumber(chNumber).Build();

        var response = await Client.GetAsync($"/api/bulkuploadorganisations?companiesHouseNumber={chNumber}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<IReadOnlyCollection<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.CompaniesHouseNumber == chNumber)
            .Which.Name.Should().Be("Bulk CH Co");
    }

    [Fact]
    public async Task GetOrganisationByName_Returns200()
    {
        var name = $"Unique BulkName {Guid.NewGuid():N}";
        await Builder.Organisation(name).Build();

        var response = await Client.GetAsync($"/api/bulkuploadorganisations/organisation-by-name?companiesHouseName={Uri.EscapeDataString(name)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<IReadOnlyCollection<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.Name == name);
    }

    [Fact]
    public async Task GetOrganisationByExternalId_Returns200WithOrg()
    {
        var externalId = Guid.NewGuid();
        await Builder.Organisation("Bulk ExternalId Co").WithExternalId(externalId).Build();

        var response = await Client.GetAsync($"/api/bulkuploadorganisations/organisation-by-externalId?externalId={externalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Service backs this with GetOrganisationByExternalId (the same lossy mapper used by the
        // singular sibling on OrganisationsController) — only Name and OrganisationNumber land.
        var payload = await response.ReadJson<OrganisationDetailModel>();
        payload.Name.Should().Be("Bulk ExternalId Co");
    }

    // Same null-deref behaviour as the sibling on OrganisationsController:
    // OrganisationService.GetOrganisationByExternalId hands null to the mapper → 500.
    [Fact]
    public async Task GetOrganisationByExternalId_WhenUnknown_CurrentlyReturns500()
    {
        var response = await Client.GetAsync(
            $"/api/bulkuploadorganisations/organisation-by-externalId?externalId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetOrganisationByReferenceNumber_Returns200()
    {
        var refNum = $"BR{Random.Shared.Next(100000, 999999)}";
        await Builder.Organisation("Bulk Ref Co").WithReferenceNumber(refNum).Build();

        var response = await Client.GetAsync(
            $"/api/bulkuploadorganisations/organisation-by-reference-number?referenceNumber={refNum}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<IReadOnlyCollection<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.ReferenceNumber == refNum)
            .Which.Name.Should().Be("Bulk Ref Co");
    }

    [Fact]
    public async Task GetOrganisationByReferenceNumber_WhenUnknown_Returns204()
    {
        var response = await Client.GetAsync(
            $"/api/bulkuploadorganisations/organisation-by-reference-number?referenceNumber=NOPE-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
