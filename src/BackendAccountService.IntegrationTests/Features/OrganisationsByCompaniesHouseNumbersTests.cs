using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class OrganisationsByCompaniesHouseNumbersTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    private const string Endpoint = "/api/organisations/organisations-by-companies-house-numbers";

    private static string NewCompaniesHouseNumber() => Guid.NewGuid().ToString("N")[..12];

    [Fact]
    public async Task PostingTwoValidCompaniesHouseNumbers_ReturnsBothMatches()
    {
        var (chn1, chn2) = (NewCompaniesHouseNumber(), NewCompaniesHouseNumber());
        await Builder.Organisation("Acme Scheme Ltd").WithCompaniesHouseNumber(chn1).WithReferenceNumber("530201").Build();
        await Builder.Organisation("Beta Scheme Co").WithCompaniesHouseNumber(chn2).WithReferenceNumber("530202").Build();

        var response = await Client.PostAsJsonAsync(Endpoint, new { companiesHouseNumbers = new[] { chn1, chn2 } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<List<OrganisationResponseModel>>();
        payload.Should().BeEquivalentTo(new[]
        {
            new { Name = "Acme Scheme Ltd", CompaniesHouseNumber = chn1, ReferenceNumber = "530201" },
            new { Name = "Beta Scheme Co", CompaniesHouseNumber = chn2, ReferenceNumber = "530202" },
        }, opts => opts.WithoutStrictOrdering()
            .Including(o => o.Name)
            .Including(o => o.CompaniesHouseNumber)
            .Including(o => o.ReferenceNumber));
    }

    [Fact]
    public async Task PostingOneHitAndOneMiss_ReturnsOnlyTheHit()
    {
        var hitChn = NewCompaniesHouseNumber();
        var missChn = NewCompaniesHouseNumber();
        await Builder.Organisation("Gamma Scheme Inc").WithCompaniesHouseNumber(hitChn).WithReferenceNumber("530203").Build();

        var response = await Client.PostAsJsonAsync(Endpoint, new { companiesHouseNumbers = new[] { hitChn, missChn } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<List<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.CompaniesHouseNumber == hitChn && o.ReferenceNumber == "530203");
    }

    [Fact]
    public async Task PostingAllMissingNumbers_ReturnsEmpty()
    {
        var (missA, missB) = (NewCompaniesHouseNumber(), NewCompaniesHouseNumber());

        var response = await Client.PostAsJsonAsync(Endpoint, new { companiesHouseNumbers = new[] { missA, missB } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<List<OrganisationResponseModel>>();
        payload.Should().BeEmpty();
    }

    [Fact]
    public async Task PostingEmptyList_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new { companiesHouseNumbers = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostingDuplicateNumbers_ReturnsSingleMatch()
    {
        var hitChn = NewCompaniesHouseNumber();
        var missChn = NewCompaniesHouseNumber();
        await Builder.Organisation("Duplicate Scheme Co").WithCompaniesHouseNumber(hitChn).WithReferenceNumber("530204").Build();

        var response = await Client.PostAsJsonAsync(
            Endpoint,
            new { companiesHouseNumbers = new[] { hitChn, hitChn, missChn, missChn } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.CompaniesHouseNumber == hitChn);
    }
}
