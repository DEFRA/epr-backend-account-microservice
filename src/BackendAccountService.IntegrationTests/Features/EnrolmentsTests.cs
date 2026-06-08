using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Features;

public class EnrolmentsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RemoveEnrolment_WithoutAuth_Returns403()
    {
        var response = await Client.DeleteAsync(
            $"/api/enrolments/{Guid.NewGuid()}?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&serviceRoleId={ServiceRoleConst.Packaging.ApprovedPerson.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveEnrolment_WithEmptyUserId_Returns400()
    {
        var response = await Client.DeleteAsync(
            $"/api/enrolments/{Guid.NewGuid()}?userId={Guid.Empty}&organisationId={Guid.NewGuid()}&serviceRoleId={ServiceRoleConst.Packaging.ApprovedPerson.Id}");

        // [NotDefault] should reject Guid.Empty before reaching the auth check.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemovePersonConnectionAndEnrolment_WhenNothingToRemove_Returns500()
    {
        // The endpoint documents 204 on success, 500 on failure — no real setup means service returns
        // false and the endpoint Problems with 500. Proves the route + binding line up; happy-path
        // success requires the deeper RoleManagement fixture that this bucket defers to a follow-up.
        var response = await Client.DeleteAsync(
            $"/api/enrolments/v1/{Guid.NewGuid()}?userId={Guid.NewGuid()}&organisationId={Guid.NewGuid()}&enrolmentId=1");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AcceptApprovedPersonNomination_NonPackagingService_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/enrolments/{Guid.NewGuid()}/approved-person-acceptance?serviceKey=NotPackaging",
            new AcceptNominationForApprovedPersonRequest());

        // [Required] model validation on the request body fires before the 404 service-not-supported
        // check the controller eventually does — so the wire sees 400, not 404.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AcceptApprovedPersonNomination_UnknownEnrolment_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/enrolments/{Guid.NewGuid()}/approved-person-acceptance?serviceKey={ServiceKeys.Packaging}",
            new AcceptNominationForApprovedPersonRequest { JobTitle = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AcceptDelegatedPersonNomination_NonPackagingService_Returns400()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.PutAsJsonAsync(
            $"/api/enrolments/{Guid.NewGuid()}/delegated-person-acceptance?serviceKey=NotPackaging",
            new AcceptNominationRequest());

        // Same shape as the ApprovedPerson sibling: [Required] body validation fires before the
        // 404 service-not-supported branch.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDelegatedPersonNominator_UnknownEnrolment_Returns404()
    {
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var response = await client.GetAsync(
            $"/api/enrolments/{Guid.NewGuid()}/delegated-person-nominator?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptApprovedPersonNomination_ForNominatedEnrolment_Returns200()
    {
        // Service filters: enrolment matched by externalId, X-EPR-User user, org, ApprovedPerson
        // role, Nominated status. Builds exactly that shape and posts the acceptance.
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Nominated);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.PutAsJsonAsync(
            $"/api/enrolments/{enrolment.ExternalId}/approved-person-acceptance?serviceKey={ServiceKeys.Packaging}",
            new AcceptNominationForApprovedPersonRequest
            {
                JobTitle = "Compliance Lead",
                Telephone = "01234567890",
                DeclarationFullName = "Test Declarant",
                DeclarationTimeStamp = DateTime.UtcNow,
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AcceptDelegatedPersonNomination_ForNominatedEnrolment_Returns200()
    {
        // Same shape as AcceptApprovedPersonNomination but for DelegatedPerson role. The service
        // dereferences enrolment.DelegatedPersonEnrolment.NomineeDeclaration, so the setup must
        // attach a DelegatedPersonEnrolment row.
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await WithDbContext(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Nominated);
            enrolment.DelegatedPersonEnrolment = new DelegatedPersonEnrolment
            {
                NominatorEnrolmentId = enrolment.Id,
            };
            await ctx.SaveChangesAsync(TestBuilders.BuilderServiceId);
        });

        var client = ClientFor(enrolment.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.PutAsJsonAsync(
            $"/api/enrolments/{enrolment.ExternalId}/delegated-person-acceptance?serviceKey={ServiceKeys.Packaging}",
            new AcceptNominationRequest
            {
                Telephone = "01234567890",
                NomineeDeclaration = "I accept this delegation.",
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDelegatedPersonNominator_ForDelegatedEnrolment_Returns200WithNominator()
    {
        // Build an ApprovedPerson admin (will be the nominator) and a sibling DelegatedPerson
        // enrolment whose DelegatedPersonEnrolment.NominatorEnrolment points at the admin's
        // enrolment id. Query by the delegated enrolment's externalId.
        var orgExternalId = Guid.NewGuid();
        Enrolment nominator = null!;
        Enrolment delegated = null!;
        await WithDbContext(async ctx =>
        {
            nominator = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
            delegated = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Nominated);
            delegated.DelegatedPersonEnrolment = new DelegatedPersonEnrolment
            {
                NominatorEnrolmentId = nominator.Id,
            };
            await ctx.SaveChangesAsync(TestBuilders.BuilderServiceId);
        });

        var client = ClientFor(delegated.Connection.Person.User!.UserId!.Value, orgExternalId);
        var response = await client.GetAsync(
            $"/api/enrolments/{delegated.ExternalId}/delegated-person-nominator?serviceKey={ServiceKeys.Packaging}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<DelegatedPersonNominatorResponse>();
        payload.Should().BeEquivalentTo(new
        {
            nominator.Connection.Person.FirstName,
            nominator.Connection.Person.LastName,
            OrganisationName = nominator.Connection.Organisation.Name,
        });
    }

    [Fact]
    public async Task RemoveEnrolment_AsApprovedAdminAgainstSecondPerson_Returns204()
    {
        // Caller is an ApprovedPerson admin; target is a second BasicUser. The auth check requires
        // caller != target and caller's role to be authorised to remove the target's highest role —
        // ApprovedPerson admin can remove a BasicUser.
        var caller = await Builder.Producer().Build();
        Enrolment target = null!;
        await WithDbContext(async ctx =>
        {
            target = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, caller.OrgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Employee,
                EnrolmentStatusConst.Approved);
        });

        var response = await Client.DeleteAsync(
            $"/api/enrolments/{target.Connection.Person.ExternalId}?userId={caller.UserId}&organisationId={caller.OrgExternalId}&serviceRoleId={ServiceRoleConst.Packaging.BasicUser.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
