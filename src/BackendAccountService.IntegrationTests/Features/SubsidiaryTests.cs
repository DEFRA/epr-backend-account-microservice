using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// Characterization tests: every subsidiary endpoint (both /api/organisations and
// /api/bulkuploadorganisations variants) currently null-derefs in ExecuteSubsidiaryAction when
// the (userId, parentOrganisationId) pair doesn't resolve to an enrolled approved-person, and
// the request lands on the wire as 500. Each endpoint advertises 4xx in [ProducesResponseType];
// this is a known won't-fix prod quirk.
public class SubsidiaryTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateAndAddSubsidiary_WithoutAuth_CurrentlyReturns500()
    {
        var request = new LinkOrganisationRequestModel
        {
            UserId = Guid.NewGuid(),
            ParentOrganisationId = Guid.NewGuid(),
            Subsidiary = new Core.Models.OrganisationModel
            {
                Name = "Subsidiary",
                Address = new Core.Models.AddressModel(),
                OrganisationType = Core.Models.OrganisationType.CompaniesHouseCompany,
            },
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/create-and-add-subsidiary", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AddSubsidiary_WithoutAuth_CurrentlyReturns500()
    {
        var request = new SubsidiaryAddRequestModel
        {
            UserId = Guid.NewGuid(),
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/add-subsidiary", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TerminateSubsidiary_WithoutAuth_CurrentlyReturns500()
    {
        var request = new SubsidiaryTerminateRequestModel
        {
            UserId = Guid.NewGuid(),
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/terminate-subsidiary", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task BulkCreateSubsidiaryAndAddRelationship_WithEmptyBody_Returns400()
    {
        // Unlike its bulk siblings this endpoint catches the empty-body case cleanly with a 400 —
        // probably because BulkOrganisationRequestModel has [Required] on something the others lack.
        var response = await Client.PostAsJsonAsync(
            "/api/bulkuploadorganisations/create-subsidiary-and-add-relationship",
            new BulkOrganisationRequestModel());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BulkAddSubsidiaryRelationship_WithEmptyBody_CurrentlyReturns500()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/bulkuploadorganisations/add-subsidiary-relationship",
            new BulkSubsidiaryAddRequestModel());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task BulkUpdateSubsidiaryRelationship_WithEmptyBody_CurrentlyReturns500()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/bulkuploadorganisations/update-subsidiary-relationship",
            new BulkSubsidiaryUpdateRequestModel());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task BulkTerminateSubsidiaryRelationship_WithEmptyBody_CurrentlyReturns500()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/bulkuploadorganisations/terminate-subsidiary-relationship",
            new BulkSubsidiaryTerminateRequestModel());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
