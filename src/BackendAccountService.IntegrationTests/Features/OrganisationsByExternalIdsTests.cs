using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.IntegrationTests.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using AwesomeAssertions;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;

namespace BackendAccountService.IntegrationTests.Features;

public class OrganisationsByExternalIdsTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    private const string Endpoint = "/api/organisations/organisations-by-externalIds";

    [Fact]
    public async Task PostingTwoValidExternalIds_ReturnsBothNamesAndNoMisses()
    {
        var (id1, id2) = (Guid.NewGuid(), Guid.NewGuid());
        await Builder.Organisation("Acme Widgets Ltd").WithExternalId(id1).WithReferenceNumber("100201").Build();
        await Builder.Organisation("Beta Co").WithExternalId(id2).WithReferenceNumber("100202").Build();

        var response = await Client.PostAsJsonAsync(Endpoint, new { externalIds = new[] { id1, id2 } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();
        payload.Should().BeEquivalentTo(new
        {
            Organisations = new[]
            {
                new { Name = "Acme Widgets Ltd", ExternalId = id1, ReferenceNumber = "100201" },
                new { Name = "Beta Co", ExternalId = id2, ReferenceNumber = "100202" },
            },
            NotFoundExternalIds = Array.Empty<Guid>(),
        }, opts => opts.WithoutStrictOrdering());
    }

    [Fact]
    public async Task PostingOneHitAndOneMiss_ReturnsHitAndReportsMiss()
    {
        var hitId = Guid.NewGuid();
        var missId = Guid.NewGuid();
        await Builder.Organisation("Gamma Inc").WithExternalId(hitId).WithReferenceNumber("100203").Build();

        var response = await Client.PostAsJsonAsync(Endpoint, new { externalIds = new[] { hitId, missId } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();
        payload.Should().BeEquivalentTo(new
        {
            Organisations = new[] { new { Name = "Gamma Inc", ExternalId = hitId, ReferenceNumber = "100203" } },
            NotFoundExternalIds = new[] { missId },
        });
    }

    [Fact]
    public async Task PostingAllMissingIds_ReturnsEmptyOrganisationsAndAllInNotFound()
    {
        var (missA, missB) = (Guid.NewGuid(), Guid.NewGuid());

        var response = await Client.PostAsJsonAsync(Endpoint, new { externalIds = new[] { missA, missB } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();
        payload.Should().BeEquivalentTo(new
        {
            Organisations = Array.Empty<object>(),
            NotFoundExternalIds = new[] { missA, missB },
        }, opts => opts.WithoutStrictOrdering());
    }

    [Fact]
    public async Task PostingEmptyExternalIdsList_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new { externalIds = Array.Empty<Guid>() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostingBulkRequest_FiftyMixedIds_ResolvesEveryHitAndReportsEveryMiss()
    {
        // The endpoint is the "bulk" lookup; exercise that character with a request size the
        // other tests don't cover (50 ids, alternating hit/miss).
        var orgs = Enumerable.Range(0, 25)
            .Select(i => ($"Bulk Lookup Co {i} {Guid.NewGuid():N}", Guid.NewGuid()))
            .ToArray();
        var misses = Enumerable.Range(0, 25).Select(_ => Guid.NewGuid()).ToArray();
        foreach (var (name, externalId) in orgs)
        {
            await Builder.Organisation(name).WithExternalId(externalId).Build();
        }

        var request = new { externalIds = orgs.Select(o => o.Item2).Concat(misses).ToArray() };
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();
        payload.Organisations.Should().HaveCount(25);
        payload.Organisations.Select(o => o.ExternalId).Should().BeEquivalentTo(orgs.Select(o => o.Item2));
        payload.NotFoundExternalIds.Should().BeEquivalentTo(misses);
    }

    [Fact]
    public async Task PostingDuplicateExternalIds_DedupesInBothResults()
    {
        // Duplicate IDs in the request shouldn't multiply rows in either result list. The service
        // groups by externalId before splitting hit/miss, so the response carries one entry per
        // distinct id regardless of how many times it was requested.
        var hitId = Guid.NewGuid();
        var missId = Guid.NewGuid();
        await Builder.Organisation("Duplicate Hit Co").WithExternalId(hitId).Build();

        var response = await Client.PostAsJsonAsync(
            Endpoint,
            new { externalIds = new[] { hitId, hitId, missId, missId } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();
        payload.Organisations.Should().ContainSingle(o => o.ExternalId == hitId);
        payload.NotFoundExternalIds.Should().ContainSingle().Which.Should().Be(missId);
    }

    [Fact]
    public async Task PostingExternalId_ForOrgWithMultipleRoles_ReturnsConnectedPersonsAndRoles_AndExcludesDeleted()
    {
        var builtEnrolment = await Builder.Producer().Build();

        Enrolment delegatedPersonEnrolment = null!;
        Enrolment secondDelegatedPersonEnrolment = null!;
        Enrolment basicUserEnrolment = null!;

        await SeedAsync(async ctx =>
        {
            delegatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, builtEnrolment.OrgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Employee, EnrolmentStatusConst.Enrolled);

            secondDelegatedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, builtEnrolment.OrgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Employee, EnrolmentStatusConst.Enrolled);

            basicUserEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, builtEnrolment.OrgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Employee, EnrolmentStatusConst.Enrolled);

            var deletedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, builtEnrolment.OrgExternalId,
                ServiceRoleConst.Packaging.BasicUser.Key,
                PersonRoleConst.Employee, EnrolmentStatusConst.Enrolled);
            deletedPersonEnrolment.Connection.Person.IsDeleted = true;

            var deletedConnectionEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, builtEnrolment.OrgExternalId,
                ServiceRoleConst.Packaging.DelegatedPerson.Key,
                PersonRoleConst.Employee, EnrolmentStatusConst.Enrolled);
            deletedConnectionEnrolment.Connection.IsDeleted = true;
        });

        var response = await Client.PostAsJsonAsync(
            Endpoint,
            new { externalIds = new[] { builtEnrolment.OrgExternalId } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationsByExternalIdsResponse>();

        var organisation = payload.Organisations
            .Should().ContainSingle(o => o.ExternalId == builtEnrolment.OrgExternalId)
            .Subject;

        organisation.People.Should().BeEquivalentTo(new[]
        {
            new
            {
                builtEnrolment.Person.FirstName,
                builtEnrolment.Person.LastName,
                builtEnrolment.Person.Email,
                TelephoneNumber = builtEnrolment.Person.Telephone,
                ServiceRole = "Approved Person",
            },
            new
            {
                delegatedPersonEnrolment.Connection.Person.FirstName,
                delegatedPersonEnrolment.Connection.Person.LastName,
                delegatedPersonEnrolment.Connection.Person.Email,
                TelephoneNumber = delegatedPersonEnrolment.Connection.Person.Telephone,
                ServiceRole = "Delegated Person",
            },
            new
            {
                secondDelegatedPersonEnrolment.Connection.Person.FirstName,
                secondDelegatedPersonEnrolment.Connection.Person.LastName,
                secondDelegatedPersonEnrolment.Connection.Person.Email,
                TelephoneNumber = secondDelegatedPersonEnrolment.Connection.Person.Telephone,
                ServiceRole = "Delegated Person",
            },
            new
            {
                basicUserEnrolment.Connection.Person.FirstName,
                basicUserEnrolment.Connection.Person.LastName,
                basicUserEnrolment.Connection.Person.Email,
                TelephoneNumber = basicUserEnrolment.Connection.Person.Telephone,
                ServiceRole = "Basic User",
            },
        });
    }
}
