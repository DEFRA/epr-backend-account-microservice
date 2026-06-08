using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController enrolment-management endpoints: UpdateEnrolment, RemoveApprovedPerson,
// AddRemoveApprovedUser. All walled by ExecuteProtectedAction (nation match required).
public class RegulatorsEnrolmentTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateEnrolment_WithUnknownEnrolment_Returns400()
    {
        // manage-enrolment resolves the org from enrolmentId *before* ExecuteProtectedAction —
        // unknown enrolment → org resolves as Guid.Empty → empty-guid short-circuit returns 400.
        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = Guid.NewGuid(),
            EnrolmentId = Guid.NewGuid(),
            EnrolmentStatus = "Approved",
            RegulatorComment = "n/a",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/accounts/manage-enrolment", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateEnrolment_ForExistingApprovedEnrolment_Returns204()
    {
        // Re-approve the producer's already-approved ApprovedPerson enrolment. The service only
        // accepts "Approved" or "Rejected" as EnrolmentStatus; "Approved" is idempotent and avoids
        // needing the SoftDeleteOrganisation cascade that "Rejected" triggers.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var request = new ManageRegulatorEnrolmentRequest
        {
            UserId = regulator.UserId,
            EnrolmentId = producer.Enrolment.ExternalId,
            EnrolmentStatus = "Approved",
            RegulatorComment = "ok",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/accounts/manage-enrolment", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveApprovedPerson_NonRegulatorUser_Returns403()
    {
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/remove-approved-users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveApprovedPerson_ForBuiltApprovedConnection_Returns200()
    {
        // The producer's ApprovedPerson connection is removable by a nation-matched regulator.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var request = new ApprovedUserRequest
        {
            UserId = regulator.UserId,
            OrganisationId = producer.OrgExternalId,
            RemovedConnectionExternalId = producer.Enrolment.Connection.ExternalId,
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/remove-approved-users", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<AssociatedPersonResponseModel>>();
        payload.Should().Contain(p => p.Email == producer.Person.Email);
    }

    [Fact]
    public async Task AddRemoveApprovedUser_NonRegulatorUser_Returns403()
    {
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = Guid.NewGuid(),
            AddingOrRemovingUserId = Guid.NewGuid(),
            AddingOrRemovingUserEmail = $"caller-{Guid.NewGuid():N}@example.com",
            InvitedPersonEmail = $"invited-{Guid.NewGuid():N}@example.com",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/add-remove-approved-users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddRemoveApprovedUser_AsRegulator_Returns200WithInviteToken()
    {
        // Invite-only path: no RemovedConnectionExternalId. The invited email is unique so
        // IsUserInvitedAsync returns false and the controller delegates to the add branch.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();

        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = producer.OrgExternalId,
            AddingOrRemovingUserId = regulator.UserId,
            AddingOrRemovingUserEmail = regulator.Person.User!.Email!,
            InvitedPersonEmail = $"invited-{Guid.NewGuid():N}@example.com",
        };

        var response = await Client.PostAsJsonAsync("/api/regulators/add-remove-approved-users", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<AddRemoveApprovedPersonResponseModel>();
        payload.OrganisationReferenceNumber.Should().Be(producer.Organisation.ReferenceNumber);
        payload.InviteToken.Should().NotBeNullOrWhiteSpace();
    }
}
