using BackendAccountService.Data.Entities;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Bundle returned by <see cref="SchemeOperatorWithAdminBuilder"/>: the operator-side
/// ApprovedPerson admin (<c>UserId</c> / <c>OperatorOrgExternalId</c> for X-EPR-User and
/// route params) and the ComplianceScheme entity (for ComplianceSchemeId params).
/// </summary>
public sealed record BuiltComplianceSchemeOperatorAdmin(BuiltEnrolment Admin, ComplianceScheme Scheme)
{
    public Guid UserId => Admin.UserId;
    public Guid OperatorOrgExternalId => Admin.OrgExternalId;
    public Guid SchemeExternalId => Scheme.ExternalId;
}
