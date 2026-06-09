using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController users endpoints: /organisations/users and /users-by-organisation-external-id.
// Both walled by ExecuteProtectedAction (regulator-nation must match the target org's nation).
public class RegulatorsUsersTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetUsersForOrganisation_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/organisations/users?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&getApprovedUsersOnly=false");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsersForOrganisation_AsRegulatorAgainstMatchingNationOrg_Returns200()
    {
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/organisations/users?userId={regulator.UserId}&organisationId={producer.OrgExternalId}&getApprovedUsersOnly=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<OrganisationUsersResponseModel>>();
        payload.Should().Contain(u => u.Email == producer.Person.Email);
    }

    [Fact]
    public async Task GetUsersByOrganisationExternalId_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/users-by-organisation-external-id?userId={Guid.NewGuid()}&externalId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsersByOrganisationExternalId_AsRegulatorAgainstMatchingNationOrg_Returns200()
    {
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/users-by-organisation-external-id?userId={regulator.UserId}&externalId={producer.OrgExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // OrganisationService.GetProducerUsers explicitly excludes ApprovedPerson enrolments
        // (`ServiceRoleId != ApprovedPerson.Id`), so the built producer admin doesn't appear.
        // The 200 + valid-JSON payload proves the auth + projection wired up; emptiness is fine.
        var payload = await response.ReadJson<List<OrganisationUserOverviewResponseModel>>();
        payload.Should().NotBeNull();
    }
}
