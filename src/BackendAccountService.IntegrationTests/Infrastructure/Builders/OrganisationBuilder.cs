using BackendAccountService.Data.Entities;
using OrganisationTypeConst = BackendAccountService.Data.DbConstants.OrganisationType;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a bare Organisation row — no Person, User, or Enrolment attached. Use for tests
/// against open lookup endpoints (org-by-CH-number, org-by-external-id, etc.) where the
/// caller doesn't need the auth shape that <see cref="ProducerBuilder"/> brings.
/// </summary>
public sealed class OrganisationBuilder(TestBuilders builders, string name)
{
    private Guid? _externalId;
    private string? _companiesHouseNumber;
    private string? _referenceNumber;
    private int _organisationTypeId = OrganisationTypeConst.CompaniesHouseCompany;
    private int? _nationId;

    public OrganisationBuilder WithExternalId(Guid externalId) { _externalId = externalId; return this; }
    public OrganisationBuilder WithCompaniesHouseNumber(string companiesHouseNumber) { _companiesHouseNumber = companiesHouseNumber; return this; }
    public OrganisationBuilder WithReferenceNumber(string referenceNumber) { _referenceNumber = referenceNumber; return this; }
    public OrganisationBuilder WithType(int organisationTypeId) { _organisationTypeId = organisationTypeId; return this; }
    public OrganisationBuilder InNation(int nationId) { _nationId = nationId; return this; }

    public async Task<Organisation> Build()
    {
        Organisation org = null!;
        await builders.WithDbContextAsync(ctx =>
        {
            org = new Organisation
            {
                Name = name,
                ExternalId = _externalId ?? Guid.NewGuid(),
                CompaniesHouseNumber = _companiesHouseNumber,
                ReferenceNumber = _referenceNumber,
                OrganisationTypeId = _organisationTypeId,
                NationId = _nationId,
            };
            ctx.Organisations.Add(org);
            return Task.CompletedTask;
        }, save: true);
        return org;
    }
}
