using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;

namespace BackendAccountService.IntegrationTests.Features;

public class OrganisationLookupTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetOrganisationsByCompaniesHouseNumber_Returns200WithMatchingOrgs()
    {
        var chNumber = $"CH-{Guid.NewGuid():N}".Substring(0, 12);
        await Builder.Organisation("Companies House Co").WithCompaniesHouseNumber(chNumber).Build();

        var response = await Client.GetAsync($"/api/organisations?companiesHouseNumber={chNumber}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<IReadOnlyCollection<OrganisationResponseModel>>();
        payload.Should().ContainSingle(o => o.CompaniesHouseNumber == chNumber)
            .Which.Name.Should().Be("Companies House Co");
    }

    // Characterization test: this endpoint advertises 200/204/400 but returns 400 even for a valid
    // CH lookup that the unprefixed sibling (GET /api/organisations?companiesHouseNumber=) and the
    // BulkUpload equivalent both handle correctly. Locking in current behaviour so any change
    // forces a deliberate re-pin.
    // Probed body: middleware wraps the InvalidOperationException from EF's LINQ evaluator with
    // detail "An exception was thrown while attempting to evaluate the LINQ query parameter
    // expression '…relationships.FirstOrDefault().FirstOrganisationId'". So the 400 is genuine —
    // the service code at OrganisationService.GetByCompaniesHouseNumberAsync dereferences
    // FirstOrDefault() without a null check when an org has an empty OrganisationRelationships
    // collection (i.e. any standalone producer org).
    [Fact]
    public async Task GetOrganisationByCompaniesHouseNumber_Singular_CurrentlyReturns400()
    {
        var chNumber = $"SH-{Guid.NewGuid():N}".Substring(0, 12);
        await Builder.Organisation("Single CH Co").WithCompaniesHouseNumber(chNumber).Build();

        var response = await Client.GetAsync($"/api/organisations/organisation-by-companies-house-number?companiesHouseNumber={chNumber}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganisationByExternalId_Returns200WithOrg()
    {
        var externalId = Guid.NewGuid();
        await Builder.Organisation("External Id Co").WithExternalId(externalId).Build();

        var response = await Client.GetAsync($"/api/organisations/organisation-by-externalId?externalId={externalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Endpoint advertises OrganisationResponseModel via [ProducesResponseType], but the service
        // actually returns OrganisationDetailModel, and the mapping only populates Name and
        // OrganisationNumber (= ReferenceNumber) — the model's Id/ExternalId/etc. are all default.
        // So we can only assert against the two fields the mapping actually copies.
        var payload = await response.ReadJson<OrganisationDetailModel>();
        payload.Name.Should().Be("External Id Co");
    }

    // Characterization test: endpoint advertises 204 on miss, but
    // OrganisationService.GetOrganisationByExternalId returns OrganisationMappings.GetOrganisationDetailModel(null),
    // which null-derefs on null.Name → middleware doesn't catch NullReferenceException → 500.
    [Fact]
    public async Task GetOrganisationByExternalId_WhenUnknown_CurrentlyReturns500()
    {
        var response = await Client.GetAsync($"/api/organisations/organisation-by-externalId?externalId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetOrganisationByReferenceNumber_Returns200()
    {
        var refNum = $"R{Random.Shared.Next(100000, 999999)}";
        await Builder.Organisation("Reference Number Co").WithReferenceNumber(refNum).Build();

        var response = await Client.GetAsync($"/api/organisations/organisation-by-reference-number?referenceNumber={refNum}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<OrganisationResponseModel>();
        payload.Should().BeEquivalentTo(new { Name = "Reference Number Co", ReferenceNumber = refNum });
    }

    // Characterization test: endpoint advertises 204 on miss, but the
    // GetOrganisationNameByInviteTokenAsync service null-derefs on `user.Email` when no user matches
    // the token. The thrown exception lands as 400 by the time it reaches the wire (likely via the
    // middleware's InvalidOperationException handler from a deeper EF call).
    [Fact]
    public async Task GetOrganisationByInviteToken_WhenUnknown_CurrentlyReturns400()
    {
        var response = await Client.GetAsync($"/api/organisations/organisation-by-invite-token?token=nope-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPersonEmails_WithEmptyOrganisationId_Returns400()
    {
        var response = await Client.GetAsync($"/api/organisations/person-emails?organisationId={Guid.Empty}&entityTypeCode=anything");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPersonEmails_AsDirectRegistrant_Returns200WithBuiltPerson()
    {
        // entityTypeCode=DR routes through the OrganisationService Direct-Registrant query —
        // Person → PersonOrganisationConnection → Organisation(ExternalId=producer org) → Enrolment.
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/organisations/person-emails?organisationId={producer.OrgExternalId}&entityTypeCode=DR");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<List<PersonEmailResponseModel>>();
        payload.Should().ContainSingle(p =>
            p.FirstName == producer.Person.FirstName
            && p.LastName == producer.Person.LastName
            && p.Email == producer.Person.Email);
    }

    [Fact]
    public async Task ValidateIssuedEprId_WithEmptyExternalId_Returns400()
    {
        var response = await Client.GetAsync($"/api/organisations/validate-issued-epr-id?externalId={Guid.Empty}&entityTypeCode=any");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateIssuedEprId_AsDirectRegistrant_Returns200WithEchoedExternalId()
    {
        // entityTypeCode=DR routes IsExternalIdExists to Organisations.Any(ExternalId == externalId);
        // the built org satisfies that. Endpoint echoes the externalId on hit.
        var producer = await Builder.Producer().Build();

        var response = await Client.GetAsync(
            $"/api/organisations/validate-issued-epr-id?externalId={producer.OrgExternalId}&entityTypeCode=DR");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var echoed = (await response.Content.ReadAsStringAsync()).Trim('"');
        Guid.Parse(echoed).Should().Be(producer.OrgExternalId);
    }

}
