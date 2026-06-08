using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class AccountCreationTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateProducerAccount_WithUnknownServiceRole_Returns400()
    {
        var model = RandomModelData.GetAccountModel(serviceRoleKey: "Not.A.Real.Role");

        var response = await Client.PostAsJsonAsync("/api/producer-accounts", model);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProducerAccount_WithValidServiceRoleAndFreshUser_Returns200WithReferenceNumber()
    {
        var model = RandomModelData.GetAccountModel(
            serviceRoleKey: BackendAccountService.Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key);

        var response = await Client.PostAsJsonAsync("/api/producer-accounts", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<Core.Models.Responses.CreateAccountResponse>();
        payload.OrganisationId.Should().NotBeEmpty();
        payload.ReferenceNumber.Should().NotBeNullOrWhiteSpace();
    }

    // Characterization test: endpoint advertises 400 for unknown email, but the service throws a
    // NullReferenceException upstream of the email-not-found guard → 500. Locking in the current
    // wire contract so any incidental fix triggers a deliberate update.
    [Fact]
    public async Task CreateApprovedUserAccount_WithUnknownEmail_CurrentlyReturns500()
    {
        var model = new ApprovedUserAccountModel
        {
            Connection = new ConnectionModel
            {
                ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key,
                JobTitle = "Title",
            },
            Person = new PersonModel
            {
                FirstName = "First",
                LastName = "Last",
                ContactEmail = $"no-such-user-{Guid.NewGuid():N}@example.com",
                TelephoneNumber = "0123",
            },
            Organisation = new OrganisationModel
            {
                Name = "Org",
                Address = new AddressModel(),
                OrganisationType = Core.Models.OrganisationType.CompaniesHouseCompany,
            },
            UserId = Guid.NewGuid(),
        };

        var response = await Client.PostAsJsonAsync("/api/producer-accounts/ApprovedUser", model);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task EnrolInvitedUser_WithUnknownInvite_Returns204()
    {
        var request = new EnrolInvitedUserRequest
        {
            UserId = Guid.NewGuid(),
            Email = $"unknown-{Guid.NewGuid():N}@example.com",
            InviteToken = "not-a-real-token",
            FirstName = "First",
            LastName = "Last",
        };

        var response = await Client.PostAsJsonAsync("/api/accounts-management/enrol-invited-user", request);

        // The endpoint advertises 204 for "Invite not found" (the Problem with NoContent status).
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EnrolInvitedUser_WithInvalidBody_Returns400()
    {
        // Empty body fails the [Required]/[NotDefault] validators.
        var response = await Client.PostAsJsonAsync("/api/accounts-management/enrol-invited-user", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EnrolInvitedUser_WithInvitedEnrolment_Returns204()
    {
        // The service requires a User with (Email, InviteToken) matching the request, joined to a
        // Person that has an Invited-status enrolment. Seeds those, then asserts the controller
        // returns 204 (success path is "NoContent").
        var token = $"enrol-{Guid.NewGuid():N}";
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                Data.DbConstants.ServiceRole.Packaging.BasicUser.Key,
                Data.DbConstants.PersonRole.Admin,
                Data.DbConstants.EnrolmentStatus.Invited);
            enrolment.Connection.Person.User!.InviteToken = token;
            await ctx.SaveChangesAsync(TestBuilders.BuilderServiceId);
        });

        var request = new EnrolInvitedUserRequest
        {
            UserId = enrolment.Connection.Person.User!.UserId!.Value,
            Email = enrolment.Connection.Person.User.Email,
            InviteToken = token,
            FirstName = "Enrolled-First",
            LastName = "Enrolled-Last",
        };

        var response = await Client.PostAsJsonAsync("/api/accounts-management/enrol-invited-user", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
