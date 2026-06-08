using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;
using NationConst = BackendAccountService.Data.DbConstants.Nation;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController organisation-level operations: nation transfer, search-by-term,
// and get-by-external-id. Mix of ExecuteProtectedAction (nation-match) and IsRegulator walls.
public class RegulatorsOrganisationTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task TransferOrganisationNation_NonRegulatorUser_Returns403()
    {
        var request = new OrganisationTransferNationRequest
        {
            UserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            TransferNationId = 2,
            TransferComments = "transfer-test",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/organisation/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TransferOrganisationNation_ForBuiltProducer_Returns204()
    {
        // Transfer the Scotland producer to England. Service writes a RegulatorComment row keyed
        // on the producer's ApprovedPerson enrolment, so Builder.Producer()'s enrolment shape
        // is exactly what's needed.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var request = new OrganisationTransferNationRequest
        {
            UserId = regulator.UserId,
            OrganisationId = producer.OrgExternalId,
            TransferNationId = NationConst.England,
            TransferComments = "transfer-test",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/organisation/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetOrganisationsBySearchTerm_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/get-organisations-by-search-term?userId={Guid.NewGuid()}&currentPage=1&pageSize=10&query=acme");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOrganisationsBySearchTerm_AsRegulator_Returns200()
    {
        // Search is paginated and filtered by nationId, so the built producer's nation must match
        // the regulator's. Use the producer's own name as the search term to guarantee a hit.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/get-organisations-by-search-term?userId={regulator.UserId}&currentPage=1&pageSize=10&query={Uri.EscapeDataString(producer.Organisation.Name)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationsSearchResultPage>();
        payload.Items.Should().Contain(o => o.ExternalId == producer.OrgExternalId);
    }

    private sealed record OrganisationsSearchResultPage(List<OrganisationSearchResult> Items, int CurrentPage, int TotalItems, int PageSize);

    [Fact]
    public async Task GetOrganisationByExternalId_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/organisation-by-external-id?organisationId={Guid.NewGuid()}&userId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOrganisationByExternalId_AsRegulatorAgainstMatchingNationOrg_Returns200()
    {
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/organisation-by-external-id?organisationId={producer.OrgExternalId}&userId={regulator.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<CompanySearchDetailsModel>();
        payload.Company.OrganisationName.Should().Be(producer.Organisation.Name);
    }
}
