using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Features;

public class PersonsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPersonByUserId_WhenPersonExists_Returns200WithPerson()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/persons?userId={producer.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<PersonResponseModel>();
        payload.Should().BeEquivalentTo(new
        {
            producer.UserId,
            producer.Person.FirstName,
            producer.Person.LastName,
        });
    }

    [Fact]
    public async Task GetPersonByUserId_WhenUnknown_Returns204()
    {
        var response = await Client.GetAsync($"/api/persons?userId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAllPersonByUserId_WhenPersonExists_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/persons/allpersons?userId={producer.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<PersonResponseModel>();
        payload.UserId.Should().Be(producer.UserId);
    }

    [Fact]
    public async Task GetPersonByExternalId_WhenPersonExists_Returns200()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync($"/api/persons/person-by-externalId?externalId={producer.Person.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<PersonResponseModel>();
        payload.FirstName.Should().Be(producer.Person.FirstName);
    }

    [Fact]
    public async Task GetPersonByExternalId_WhenUnknown_Returns204()
    {
        var response = await Client.GetAsync($"/api/persons/person-by-externalId?externalId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetPersonByInviteToken_WhenUnknownToken_Returns204()
    {
        // No user/person with this invite token exists, and no matching invitation token on User
        var response = await Client.GetAsync($"/api/persons/person-by-invite-token?token=no-such-token-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetPersonByInviteToken_WhenInvitedEnrolmentExists_Returns200WithEmailAndOrg()
    {
        // PersonService.GetPersonServiceRoleByInviteTokenAsync joins User (by InviteToken) ×
        // Person × PersonOrganisationConnection × Organisation × Enrolment(EnrolmentStatusId=Invited).
        // Builds an Invited enrolment, fixes up the User.InviteToken, then queries by that token.
        var token = $"invite-{Guid.NewGuid():N}";
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Invited);
            enrolment.Connection.Person.User!.InviteToken = token;
            await ctx.SaveChangesAsync(TestBuilders.BuilderServiceId);
        });

        var response = await Client.GetAsync($"/api/persons/person-by-invite-token?token={token}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<InviteApprovedUserModel>();
        payload.Email.Should().Be(enrolment.Connection.Person.User!.Email);
        payload.OrganisationId.Should().Be(enrolment.Connection.Organisation.ReferenceNumber);
    }

}
