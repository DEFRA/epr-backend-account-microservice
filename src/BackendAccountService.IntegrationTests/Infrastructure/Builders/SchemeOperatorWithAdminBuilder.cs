using BackendAccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a compliance-scheme operator and an ApprovedPerson admin enrolled on it. Returned
/// bundle exposes both the user-side identifiers (<c>UserId</c>, <c>OperatorOrgExternalId</c>)
/// and the backing scheme for scheme-id request params.
/// </summary>
public sealed class SchemeOperatorWithAdminBuilder(TestBuilders builders)
{
    public async Task<BuiltComplianceSchemeOperatorAdmin> Build()
    {
        var orgExternalId = Guid.NewGuid();
        ComplianceScheme scheme = null!;
        Enrolment enrolment = null!;
        await builders.WithDbContextAsync(async ctx =>
        {
            scheme = await ctx.ComplianceSchemes
                .Where(cs => !cs.IsDeleted)
                .OrderBy(cs => cs.Id)
                .FirstAsync();
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
            var org = enrolment.Connection.Organisation;
            org.IsComplianceScheme = true;
            org.CompaniesHouseNumber = scheme.CompaniesHouseNumber;
            enrolment.Connection.Person.User!.ExternalIdpUserId =
                enrolment.Connection.Person.User.UserId!.Value.ToString();
        }, save: true);
        return new BuiltComplianceSchemeOperatorAdmin(new BuiltEnrolment(enrolment), scheme);
    }
}
