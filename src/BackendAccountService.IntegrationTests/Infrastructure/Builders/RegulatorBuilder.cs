using BackendAccountService.Data.Entities;
using NationConst = BackendAccountService.Data.DbConstants.Nation;
using OrganisationTypeConst = BackendAccountService.Data.DbConstants.OrganisationType;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a regulator-admin user (ServiceRole Regulator.Admin, Approved, on an organisation
/// typed Regulators). Default nation is Scotland to align with <see cref="ProducerBuilder"/>;
/// override via <see cref="InNation"/> for cross-nation paths.
/// </summary>
public sealed class RegulatorBuilder(TestBuilders builders)
{
    private int _nationId = NationConst.Scotland;

    public RegulatorBuilder InNation(int nationId)
    {
        _nationId = nationId;
        return this;
    }

    public async Task<BuiltEnrolment> Build()
    {
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await builders.WithDbContextAsync(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Regulator.Admin.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
            var org = enrolment.Connection.Organisation;
            org.OrganisationTypeId = OrganisationTypeConst.Regulators;
            org.NationId = _nationId;
            enrolment.Connection.Person.User!.ExternalIdpUserId =
                enrolment.Connection.Person.User.UserId!.Value.ToString();
        }, save: true);
        return new BuiltEnrolment(enrolment);
    }
}
