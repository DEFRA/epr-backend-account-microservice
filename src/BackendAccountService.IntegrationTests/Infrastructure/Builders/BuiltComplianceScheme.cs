using BackendAccountService.Data.Entities;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Pair of a migration-seeded ComplianceScheme + its built operator Organisation, returned by
/// <see cref="SchemeOperatorBuilder"/>.
/// </summary>
public sealed record BuiltComplianceScheme(ComplianceScheme Scheme, Organisation OperatorOrganisation)
{
    public Guid SchemeExternalId => Scheme.ExternalId;
    public Guid OperatorExternalId => OperatorOrganisation.ExternalId;
}
