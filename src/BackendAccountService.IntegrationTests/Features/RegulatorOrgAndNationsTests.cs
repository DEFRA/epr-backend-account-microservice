using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;
using NationConst = BackendAccountService.Data.DbConstants.Nation;

namespace BackendAccountService.IntegrationTests.Features;

public class RegulatorOrgAndNationsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetOrganisationIdFromNation_NoRegulatorOrg_Returns204()
    {
        var response = await Client.GetAsync("/api/regulator-organisation?nation=England");

        // Vanilla migrations don't include a regulator org for England, so the lookup is empty → 204.
        // A 200 would mean someone added a regulator-org row and this assertion needs an update.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetOrganisationIdFromNation_WithBuiltRegulatorOrg_Returns200WithOrg()
    {
        // Other tests build regulator orgs in Scotland (Builder.Regulator()'s default), and the
        // controller uses FirstOrDefault — so this test picks NorthernIreland to guarantee the
        // built row is the only regulator org for that nation across the whole assembly run.
        var regulator = await Builder.Regulator().InNation(NationConst.NorthernIreland).Build();

        var response = await Client.GetAsync("/api/regulator-organisation?nation=Northern Ireland");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<CheckOrganisationExistResponseModel>();
        payload.Should().BeEquivalentTo(new
        {
            ExternalId = regulator.OrgExternalId,
            OrganisationName = regulator.Organisation.Name,
        });
    }

    [Fact]
    public async Task CreateRegulatorOrganisation_WithNationId_Returns201()
    {
        var request = new CreateRegulatorOrganisationRequest
        {
            ServiceId = "EprIntegrationTestService",
            Name = $"Test Regulator Org {Guid.NewGuid():N}".Substring(0, 25),
            NationId = 1,
        };

        var response = await Client.PostAsJsonAsync("/api/regulator-organisation", request);

        // Endpoint returns 201 with a Location header on success, or BuildErrorResponse() otherwise.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetNationIdsFromOrganisationId_UnknownOrg_Returns200WithZeroSentinel()
    {
        var response = await Client.GetAsync($"/api/regulator-organisation/organisation-nation?organisationId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<int>>();
        // RegulatorService.GetOrganisationNationsAsync returns a single-element list `[0]` when no
        // org matches — deliberate to "maintain original behaviour where null nation ids mean that
        // all nations are valid for this organisation". So an unknown org reads as "all nations".
        payload.Should().Equal(0);
    }

    [Fact]
    public async Task GetNationDetailsByNationId_ForKnownNation_Returns200()
    {
        var response = await Client.GetAsync("/api/nations/nation-id/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<NationDetailsResponseDto>();
        payload.Should().BeEquivalentTo(new { Name = "England", NationCode = "GB-ENG" });
    }

    [Fact]
    public async Task GetNationDetailsByNationId_OutOfRange_Returns400()
    {
        // ValidateInRange(1,4) filter rejects 99 before the action runs.
        var response = await Client.GetAsync("/api/nations/nation-id/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
