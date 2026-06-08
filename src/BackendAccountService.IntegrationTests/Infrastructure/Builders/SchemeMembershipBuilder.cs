using BackendAccountService.Data.Entities;
using InterOrgRoleConst = BackendAccountService.Data.DbConstants.InterOrganisationRole;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds an OrganisationsConnection (producer → operator) + a SelectedScheme row tied to
/// the supplied operator's ComplianceScheme. Bypasses the production /select endpoint
/// whose internal "pick FIRST operator with matching CompaniesHouseNumber" is non-deterministic
/// when prior tests leave operator orgs with the same CH around. Returns the SelectedScheme
/// externalId.
/// </summary>
public sealed class SchemeMembershipBuilder(
    TestBuilders builders,
    BuiltEnrolment producer,
    Organisation operatorOrganisation,
    ComplianceScheme scheme)
{
    public async Task<Guid> Build()
    {
        var selectedExternalId = Guid.NewGuid();
        await builders.WithDbContextAsync(ctx =>
        {
            var connection = new OrganisationsConnection
            {
                FromOrganisationId = producer.Organisation.Id,
                FromOrganisationRoleId = InterOrgRoleConst.Producer,
                ToOrganisationId = operatorOrganisation.Id,
                ToOrganisationRoleId = InterOrgRoleConst.ComplianceScheme,
            };
            ctx.OrganisationsConnections.Add(connection);
            ctx.SelectedSchemes.Add(new SelectedScheme
            {
                ExternalId = selectedExternalId,
                ComplianceSchemeId = scheme.Id,
                OrganisationConnection = connection,
            });
            return Task.CompletedTask;
        }, save: true);
        return selectedExternalId;
    }
}
