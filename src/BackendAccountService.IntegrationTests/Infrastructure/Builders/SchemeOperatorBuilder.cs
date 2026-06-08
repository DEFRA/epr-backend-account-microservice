using BackendAccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NationConst = BackendAccountService.Data.DbConstants.Nation;
using OrganisationTypeConst = BackendAccountService.Data.DbConstants.OrganisationType;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a compliance-scheme operator: an Organisation with IsComplianceScheme=true whose
/// CompaniesHouseNumber matches a non-deleted migration-seeded ComplianceScheme. The services join
/// ComplianceSchemes → Organisations on CompaniesHouseNumber and reject requests with no
/// operator-org backing the chosen scheme.
/// <para>
/// Chain <see cref="WithAdmin"/> when a test also needs a CS-operator-side user (the auth
/// shape required by <c>IsAuthorisedToViewComplianceSchemeMembers</c>).
/// </para>
/// </summary>
public sealed class SchemeOperatorBuilder(TestBuilders builders)
{
    public SchemeOperatorWithAdminBuilder WithAdmin() => new(builders);

    public async Task<BuiltComplianceScheme> Build()
    {
        ComplianceScheme scheme = null!;
        Organisation operatorOrg = null!;
        await builders.WithDbContextAsync(async ctx =>
        {
            scheme = await ctx.ComplianceSchemes
                .Where(cs => !cs.IsDeleted)
                .OrderBy(cs => cs.Id)
                .FirstAsync();
            operatorOrg = new Organisation
            {
                Name = $"Operator-{Guid.NewGuid():N}".Substring(0, 30),
                ExternalId = Guid.NewGuid(),
                CompaniesHouseNumber = scheme.CompaniesHouseNumber,
                IsComplianceScheme = true,
                OrganisationTypeId = OrganisationTypeConst.CompaniesHouseCompany,
                NationId = NationConst.Scotland,
            };
            ctx.Organisations.Add(operatorOrg);
        }, save: true);
        return new BuiltComplianceScheme(scheme, operatorOrg);
    }
}
