using System.Net;
using System.Net.Http.Json;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.IntegrationTests.Infrastructure;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.IntegrationTests.Features;

// RegulatorsController service-callable POSTs: no IsRegulator wall — the controller only
// null-checks the body and then delegates to the service. With random Reference/Email values
// the service Result<T> fails and BuildErrorResponse maps it to a non-200 status; with matching
// references the service succeeds.
public class RegulatorsByServiceTests(AccountApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_WithUnknownReferences_Returns400()
    {
        var request = new ManageUserDetailsChangeRequestByService
        {
            OrganisationReference = $"REF-{Guid.NewGuid():N}",
            UserEmail = $"user-{Guid.NewGuid():N}@example.com",
            RegulatorEmail = $"reg-{Guid.NewGuid():N}@example.com",
            HasRegulatorAccepted = true,
        };

        var response = await Client.PostAsJsonAsync(
            "/api/regulators/accounts/manage-user-changes-by-service",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AcceptOrRejectUserDetailsChangeRequestByService_ForMatchingReferences_Returns200()
    {
        // Service-callable path resolves the regulator by Person.Email, the target connection by
        // (Person.Email, Organisation.ReferenceNumber), and the active ChangeHistory row by
        // (PersonId, OrganisationId). Build all three.
        var regulator = await Builder.Regulator().Build();
        var producer = await Builder.Producer().Build();
        await Builder.ChangeHistoryFor(producer).Build();

        var request = new ManageUserDetailsChangeRequestByService
        {
            OrganisationReference = producer.Organisation.ReferenceNumber!,
            UserEmail = producer.Person.Email,
            RegulatorEmail = regulator.Person.Email,
            HasRegulatorAccepted = true,
        };

        var response = await Client.PostAsJsonAsync(
            "/api/regulators/accounts/manage-user-changes-by-service",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<RegulatorUserDetailsUpdateByServiceResponse>();
        payload.HasUserDetailsChangeAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNonCompaniesHouseCompanyByService_WithUnknownReferences_Returns400()
    {
        var request = new ManageNonCompaniesHouseCompanyByService
        {
            OrganisationReference = $"REF-{Guid.NewGuid():N}",
            UserEmail = $"user-{Guid.NewGuid():N}@example.com",
            OrganisationName = "Nowhere Ltd",
            BuildingNumber = "1",
            Street = "High St",
            Postcode = "AB1 2CD",
            Town = "Somewhere",
        };

        var response = await Client.PostAsJsonAsync(
            "/api/regulators/organisations/manage-organisation-changes-by-service",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateNonCompaniesHouseCompanyByService_ForBuiltNonCompaniesHouseProducer_Returns200()
    {
        // Service requires Organisation.OrganisationTypeId == NonCompaniesHouseCompany.
        // Builder.Producer() defaults to CompaniesHouseCompany, so flip the type before
        // calling. Email + ReferenceNumber line up via the built producer.
        var producer = await Builder.Producer().Build();
        await SeedAsync(async ctx =>
        {
            var org = await ctx.Organisations.SingleAsync(o => o.Id == producer.Organisation.Id);
            org.OrganisationTypeId = Data.DbConstants.OrganisationType.NonCompaniesHouseCompany;
        });

        var request = new ManageNonCompaniesHouseCompanyByService
        {
            OrganisationReference = producer.Organisation.ReferenceNumber!,
            UserEmail = producer.Person.Email,
            OrganisationName = "Renamed NonCH Ltd",
            BuildingNumber = "10",
            Street = "New Street",
            Postcode = "NE1 1NE",
            Town = "Newtown",
        };

        var response = await Client.PostAsJsonAsync(
            "/api/regulators/organisations/manage-organisation-changes-by-service",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadJson<RegulatorOrganisationUpdateResponse>();
        payload.HasOrganisationUpdated.Should().BeTrue();
    }
}
