using System.Net;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController pending-applications endpoints: /pending-applications (IsRegulator-walled)
// and /applications/enrolments (ExecuteProtectedAction nation-match-walled).
public class RegulatorsApplicationsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPendingApplications_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/pending-applications?userId={Guid.NewGuid()}&currentPage=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingApplications_AsRegulator_Returns200()
    {
        var regulator = await Builder.Regulator().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/pending-applications?userId={regulator.UserId}&currentPage=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // PaginatedResponse<T> has private setters and a constructor whose params don't match the
        // wire-format names, so System.Text.Json can't populate the production type. Use a small
        // mirror record that names the fields the API actually emits.
        var payload = await response.ReadJson<PendingApplicationsPage>();
        payload.CurrentPage.Should().Be(1);
        payload.PageSize.Should().Be(10);
        payload.Items.Should().NotBeNull();
    }

    private sealed record PendingApplicationsPage(List<object> Items, int CurrentPage, int TotalItems, int PageSize);

    [Fact]
    public async Task GetPendingApplicationsForOrganisation_WithEmptyGuids_Returns400()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/applications/enrolments?userId={Guid.Empty}&organisationId={Guid.Empty}");

        // ExecuteProtectedAction returns 400 ("Invalid Data") before even hitting the nation check.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPendingApplicationsForOrganisation_NonRegulatorUser_Returns403()
    {
        var response = await Client.GetAsync(
            $"/api/regulators/applications/enrolments?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingApplicationsForOrganisation_AsRegulatorAgainstMatchingNationOrg_Returns200()
    {
        // Regulator (Scotland) + producer (Scotland — RandomModelData seeds Scotland). The producer's
        // ApprovedPerson admin enrolment is in EnrolmentStatus.Approved which the service projects
        // into ApplicationEnrolmentDetails.Users.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/regulators/applications/enrolments?userId={regulator.UserId}&organisationId={producer.OrgExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<ApplicationEnrolmentDetails>();
        payload.OrganisationId.Should().Be(producer.OrgExternalId);
        payload.OrganisationName.Should().Be(producer.Organisation.Name);
    }
}
