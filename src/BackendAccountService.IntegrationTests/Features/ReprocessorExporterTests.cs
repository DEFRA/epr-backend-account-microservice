using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class ReprocessorExporterTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateReExUserAccount_WithFreshUser_Returns200()
    {
        var userId = Guid.NewGuid();
        var client = ClientFor(userId, Guid.NewGuid());

        var account = new ReprocessorExporterAccount
        {
            Person = new PersonModel
            {
                FirstName = "First",
                LastName = "Last",
                ContactEmail = $"reex-{Guid.NewGuid():N}@example.com",
                TelephoneNumber = "0123",
            },
            User = new UserModel { UserId = userId, Email = "x@y.com" },
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/reprocessor-exporter-user-accounts?serviceKey={ServiceKeys.ReprocessorExporter}",
            account);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Controller returns Ok() with no body. Verify the side-effect: the persisted User row exists.
        await WithDbContext(async ctx =>
        {
            var persisted = await ctx.Persons
                .Where(p => p.User!.UserId == userId)
                .Select(p => new { p.FirstName, p.LastName, p.Email })
                .SingleOrDefaultAsync();
            persisted.Should().BeEquivalentTo(new { FirstName = "First", LastName = "Last", Email = account.Person.ContactEmail });
        });
    }

    [Fact]
    public async Task AddReExOrganisation_WithMismatchedAuditUserId_Returns400()
    {
        // ValidateUserIdsMatch fires first — auditUserId (header) != body User.UserId.
        var client = ClientFor(Guid.NewGuid(), Guid.NewGuid());

        var organisation = new
        {
            User = new { UserId = Guid.NewGuid() },
            Organisation = new { Name = "Re-Ex Co", CompaniesHouseNumber = (string?)null },
            Person = new { FirstName = "F", LastName = "L" },
            InvitedApprovedUsers = new object[] { },
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/reprocessor-exporter-accounts?serviceKey={ServiceKeys.ReprocessorExporter}",
            organisation);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganisationWithPersons_UnknownOrg_Returns404()
    {
        var response = await Client.GetAsync($"/api/organisations/organisation-with-persons/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrganisationWithPersons_ForBuiltOrg_Returns200WithPersons()
    {
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/organisations/organisation-with-persons/{producer.OrgExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationDetailsResponseDto>();
        payload.OrganisationName.Should().Be(producer.Organisation.Name);
        payload.Persons.Should().ContainSingle(p =>
            p.FirstName == producer.Person.FirstName
            && p.LastName == producer.Person.LastName
            && p.Email == producer.Person.Email);
    }

    [Fact]
    public async Task GetPersonDetailsByIds_WithValidRequest_Returns200()
    {
        var request = new PersonsDetailsRequestDto
        {
            OrgId = Guid.NewGuid(),
            UserIds = new List<Guid> { Guid.NewGuid() },
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/person-details-by-ids", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Random userIds + random orgId → service returns an empty list. Asserting BeEmpty pins
        // the wire shape (List<OrganisationPersonDto>) without coupling to setup data.
        var payload = await response.ReadJson<List<OrganisationPersonDto>>();
        payload.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPersonDetailsByIds_WithMissingFields_FailsValidation()
    {
        // Empty body — FluentValidation should reject.
        var response = await Client.PostAsJsonAsync("/api/organisations/person-details-by-ids", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
