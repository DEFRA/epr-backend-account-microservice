using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class LocalAuthorityCrudTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateLocalAuthority_ReturnsCreatedPayload()
    {
        var request = NewCreateRequest("LA-001", "Test Borough Council");

        var response = await Client.PostAsJsonAsync("/api/laorganisations/create", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Should().BeEquivalentTo(new
        {
            IsSuccess = true,
            Value = new { DistrictCode = "LA-001", Name = "Test Borough Council" },
        });
        result.Value.ExternalId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RetrieveAllLocalAuthorities_AfterCreate_IncludesNewLa()
    {
        await CreateLocalAuthorityAsync("LA-002", "Retrieve LA");

        var response = await Client.GetAsync("/api/laorganisations/retrieve");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.ReadJson<List<LocalAuthorityResponseModel>>();
        list.Should().ContainSingle(la => la.DistrictCode == "LA-002" && la.Name == "Retrieve LA");
    }

    [Fact]
    public async Task GetByDistrictCode_AfterCreate_Returns200WithLa()
    {
        await CreateLocalAuthorityAsync("LA-003", "District LA");

        var response = await Client.GetAsync("/api/laorganisations/district-code?districtCode=LA-003");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Value.Name.Should().Be("District LA");
    }

    [Fact]
    public async Task GetByExternalId_AfterCreate_Returns200()
    {
        var created = await CreateLocalAuthorityAsync("LA-004", "ExternalId LA");

        var response = await Client.GetAsync($"/api/laorganisations/external-id?id={created.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Value.DistrictCode.Should().Be("LA-004");
    }

    [Fact]
    public async Task UpdateByDistrictCode_AfterCreate_PersistsNewName()
    {
        await CreateLocalAuthorityAsync("LA-005", "Old Name");

        var updateBody = new UpdateLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            DistrictCode = "LA-005",
            Name = "New Name",
            Nation = "England",
            WasteAuthorityType = "Waste Collection Authority",
        };
        var updateResponse = await Client.PutAsJsonAsync("/api/laorganisations/update/district-code", updateBody);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync("/api/laorganisations/district-code?districtCode=LA-005");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await getResponse.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Value.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteByDistrictCode_AfterCreate_HidesLa()
    {
        await CreateLocalAuthorityAsync("LA-006", "Delete LA");

        var removeBody = new RemoveLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            DistrictCode = "LA-006",
        };
        var del = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/laorganisations/remove/district-code")
        {
            Content = JsonContent.Create(removeBody),
        });
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterGet = await Client.GetAsync("/api/laorganisations/district-code?districtCode=LA-006");
        afterGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByUnknownDistrictCode_Returns404()
    {
        var response = await Client.GetAsync("/api/laorganisations/district-code?districtCode=NOPE");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNationByName_ForKnownNation_Returns200()
    {
        var response = await Client.GetAsync("/api/laorganisations/nation-name?nationName=England");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Service returns Result.SuccessResult() (no Value) on hit, so the envelope's IsSuccess is
        // the load-bearing field; Value will be null.
        var result = await response.ReadJson<ResultEnvelope<object>>();
        result.IsSuccess.Should().BeTrue();
    }

    // ---- GET /api/laorganisations/organisation-name ----

    [Fact]
    public async Task GetByOrganisationName_AfterCreate_Returns200()
    {
        var unique = $"NameLookup LA {Guid.NewGuid():N}".Substring(0, 25);
        await CreateLocalAuthorityAsync("LA-007", unique);

        var response = await Client.GetAsync($"/api/laorganisations/organisation-name?name={Uri.EscapeDataString(unique)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Value.Name.Should().Be(unique);
    }

    [Fact]
    public async Task GetByOrganisationName_WithEmptyName_Returns400()
    {
        var response = await Client.GetAsync("/api/laorganisations/organisation-name?name=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByOrganisationName_WhenUnknown_Returns404()
    {
        var response = await Client.GetAsync(
            $"/api/laorganisations/organisation-name?name=NoSuchLA-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- GET /api/laorganisations/organisation-type-id ----

    [Fact]
    public async Task GetByOrganisationTypeId_WithNegativeId_Returns400()
    {
        var response = await Client.GetAsync("/api/laorganisations/organisation-type-id?id=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByOrganisationTypeId_WhenUnknownType_Returns404()
    {
        // 9999 isn't a seeded organisation-type id → service returns empty list → controller 404.
        var response = await Client.GetAsync("/api/laorganisations/organisation-type-id?id=9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- GET /api/laorganisations/organisation-type-name ----

    [Fact]
    public async Task GetByOrganisationTypeName_WithEmpty_Returns400()
    {
        var response = await Client.GetAsync("/api/laorganisations/organisation-type-name?organisationTypeName=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByOrganisationTypeName_WhenUnknown_Returns404()
    {
        var response = await Client.GetAsync(
            $"/api/laorganisations/organisation-type-name?organisationTypeName=NoSuchType-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- PUT /api/laorganisations/update/external-id ----

    [Fact]
    public async Task UpdateByExternalId_AfterCreate_PersistsNewName()
    {
        var created = await CreateLocalAuthorityAsync("LA-008", "Pre-rename LA");

        var updateBody = new UpdateLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            ExternalId = created.ExternalId.ToString(),
            Name = "Post-rename LA",
            Nation = "England",
            WasteAuthorityType = "Waste Collection Authority",
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/laorganisations/update/external-id", updateBody);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/laorganisations/external-id?id={created.ExternalId}");
        var result = await getResponse.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        result.Value.Name.Should().Be("Post-rename LA");
    }

    [Fact]
    public async Task UpdateByExternalId_WithInvalidGuid_Returns400()
    {
        var body = new UpdateLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            ExternalId = "not-a-guid",
            Name = "irrelevant",
            Nation = "England",
            WasteAuthorityType = "Waste Collection Authority",
        };

        var response = await Client.PutAsJsonAsync("/api/laorganisations/update/external-id", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- DELETE /api/laorganisations/remove/external-id ----

    [Fact]
    public async Task RemoveByExternalId_AfterCreate_HidesLa()
    {
        var created = await CreateLocalAuthorityAsync("LA-009", "Delete-by-ExternalId LA");

        var removeBody = new RemoveLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            ExternalId = created.ExternalId.ToString(),
        };
        var del = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/laorganisations/remove/external-id")
        {
            Content = JsonContent.Create(removeBody),
        });
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterGet = await Client.GetAsync($"/api/laorganisations/external-id?id={created.ExternalId}");
        afterGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveByExternalId_WithInvalidGuid_Returns400()
    {
        var body = new RemoveLocalAuthorityRequest
        {
            UserId = Guid.NewGuid(),
            ExternalId = "not-a-guid",
        };

        var del = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/laorganisations/remove/external-id")
        {
            Content = JsonContent.Create(body),
        });

        del.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private CreateLocalAuthorityRequest NewCreateRequest(string districtCode, string name) => new()
    {
        UserId = Guid.NewGuid(),
        DistrictCode = districtCode,
        Name = name,
        WasteAuthorityType = "Waste Collection Authority",
        Nation = "England",
        ValidatedWithCompaniesHouse = false,
        IsComplianceScheme = false,
    };

    private async Task<LocalAuthorityResponseModel> CreateLocalAuthorityAsync(string districtCode, string name)
    {
        var response = await Client.PostAsJsonAsync("/api/laorganisations/create", NewCreateRequest(districtCode, name));
        response.EnsureSuccessStatusCode();
        var result = await response.ReadJson<ResultEnvelope<LocalAuthorityResponseModel>>();
        return result.Value;
    }

    // Mirror of the wire shape of Result<T> (which serialises as { isSuccess, value, errorMessage, statusCode })
    // — Result<T>'s setters aren't public so we can't deserialise straight into it.
    private sealed record ResultEnvelope<T>(bool IsSuccess, T Value, string? ErrorMessage, string StatusCode);
}
