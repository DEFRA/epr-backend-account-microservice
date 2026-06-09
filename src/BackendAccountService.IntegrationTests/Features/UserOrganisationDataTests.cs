using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class UserOrganisationDataTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetUserOrganisations_WhenUserHasOrg_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/users/user-organisations?userId={producer.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserOrganisationsListModel>();
        payload.User.Should().BeEquivalentTo(new
        {
            producer.Person.FirstName,
            producer.Person.LastName,
            producer.Person.Email,
        });
    }

    [Fact]
    public async Task GetUserOrganisationsIncludeDeleted_WhenUserHasOrg_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/users/user-organisations-include-deleted?userId={producer.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserOrganisationsListModel>();
        payload.User.Should().BeEquivalentTo(new
        {
            producer.Person.FirstName,
            producer.Person.LastName,
            producer.Person.Email,
        });
    }

    [Fact]
    public async Task GetV1UserOrganisations_WhenUserHasOrg_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/users/v1/user-organisations?userId={producer.UserId}&serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserOrganisationsListModel>();
        payload.User.Should().BeEquivalentTo(new
        {
            producer.Person.FirstName,
            producer.Person.LastName,
            producer.Person.Email,
        });
    }

    [Fact]
    public async Task GetV1UserOrganisations_WithEmptyUserId_Returns400()
    {
        var response = await Client.GetAsync($"/api/users/v1/user-organisations?userId={Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostUserOrganisationIds_ReturnsReferenceNumberCsv()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.PostAsJsonAsync(
            "/api/users/user-organisation-ids",
            new UserOrganisationIdentifiersRequest { ObjectId = producer.UserId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UserOrganisationIdentifiersResponse>();
        // OrganisationIds is a comma-separated list of organisation *reference numbers* (not
        // external Guids) — see UserService.GetUserOrganisationIdListAsync selecting o.ReferenceNumber.
        payload.OrganisationIds.Should().Contain(producer.Organisation.ReferenceNumber!);
        payload.Action.Should().Be("Continue");
    }

    [Fact]
    public async Task GetUserAccount_ForEnrolledUser_Returns200()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);

        var response = await client.GetAsync($"/api/users/user-account?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<PersonWithOrganisationsResponse>();
        payload.Should().BeEquivalentTo(new
        {
            producer.Person.FirstName,
            producer.Person.LastName,
            producer.Person.Email,
        });
    }

    [Fact]
    public async Task UpdatePersonalDetails_WithUnsupportedService_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            "/api/users/personal-details?serviceKey=NotPackaging",
            new UpdateUserDetailsRequest { FirstName = "X", LastName = "Y" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePersonalDetails_AsApprovedAdmin_Returns200WithUpdatedDetails()
    {
        var producer = await Builder.Producer().Build();
        var client = ClientFor(producer.UserId, producer.OrgExternalId);

        var response = await client.PutAsJsonAsync(
            $"/api/users/personal-details?serviceKey={ServiceKeys.Packaging}",
            new UpdateUserDetailsRequest
            {
                FirstName = "Updated First",
                LastName = "Updated Last",
                JobTitle = "Updated Title",
                Telephone = "01234567890",
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<UpdateUserDetailsResponse>();
        // ApprovedPerson updates route through approval — basic-only flag is false, change-history
        // is populated. Asserting the change-history captured the new first name proves the
        // controller persisted the request body, not just that it responded 200.
        payload.HasBasicUserDetailsUpdated.Should().BeFalse();
        payload.ChangeHistory!.NewValues!.FirstName.Should().Be("Updated First");
        payload.ChangeHistory.NewValues.LastName.Should().Be("Updated Last");
    }
}
