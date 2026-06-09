using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// OrganisationsController maintenance/read endpoints that don't fit the lookup, users, or
// relationships clusters: UpdateOrganisation (PUT), GetUpdatedProducers (date-range read),
// GetNationCode (single-org metadata read).
public class OrganisationMaintenanceTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateOrganisation_WithoutBody_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        // PutAsJsonAsync with a null body produces a "null" JSON payload that the controller's
        // FromBody binder treats as null → controller's null-check returns 400.
        var response = await client.PutAsJsonAsync<OrganisationUpdateModel?>(
            $"/api/organisations/organisation/{Guid.NewGuid()}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrganisation_UnknownOrg_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());
        var update = new OrganisationUpdateModel { Name = "Renamed", NationId = 1 };

        var response = await client.PutAsJsonAsync(
            $"/api/organisations/organisation/{Guid.NewGuid()}",
            update);

        // Service returns Result.FailedResult for unknown org; BuildErrorResponse maps to 400.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrganisation_ForBuiltOrg_Returns200AndPersistsChanges()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);
        var update = new OrganisationUpdateModel
        {
            Name = $"Renamed {Guid.NewGuid():N}".Substring(0, 30),
            NationId = 1,
            Street = "New Street",
            Town = "New Town",
            Postcode = "NE1 1NE",
        };

        var response = await client.PutAsJsonAsync(
            $"/api/organisations/organisation/{producer.OrgExternalId}",
            update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Read the updated row back via the lookup endpoint to confirm persistence.
        var get = await Client.GetAsync($"/api/organisations/organisation-by-reference-number?referenceNumber={producer.Organisation.ReferenceNumber}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await get.ReadJson<OrganisationResponseModel>();
        refreshed.Name.Should().Be(update.Name);
    }

    [Fact]
    public async Task GetUpdatedProducers_WithEmptyRange_Returns204()
    {
        var from = "2099-01-01";
        var to = "2099-01-02";

        var response = await Client.GetAsync(
            $"/api/organisations/organisation?From={from}&To={to}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetUpdatedProducers_WithRangeCoveringNow_Returns200WithBuiltOrg()
    {
        var producer = await Builder.Producer().Build();

        // Builder.Producer()'s organisation was just persisted, so LastUpdatedOn falls inside
        // any range that brackets "now". Use the previous + next day to avoid timezone surprises.
        var from = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        var response = await Client.GetAsync(
            $"/api/organisations/organisation?From={from}&To={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<UpdatedProducersResponseModel>>();
        payload.Should().Contain(p => p.ExternalId == producer.OrgExternalId.ToString());
    }

    [Fact]
    public async Task GetUpdatedProducers_WithInvalidRange_Returns400()
    {
        // Missing required From parameter.
        var response = await Client.GetAsync("/api/organisations/organisation?To=2026-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNationCode_UnknownOrg_Returns204()
    {
        var response = await Client.GetAsync(
            $"/api/organisations/nation-code?organisationId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetNationCode_ForBuiltOrg_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/organisations/nation-code?organisationId={producer.OrgExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Builder.Producer() uses RandomModelData which sets Nation.Scotland (NationCode 'GB-SCT').
        var nationCode = (await response.Content.ReadAsStringAsync()).Trim('"');
        nationCode.Should().Be("GB-SCT");
    }
}
