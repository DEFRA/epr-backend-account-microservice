using BackendAccountService.Data.Entities;
using ServiceRoleConst = BackendAccountService.Data.DbConstants.ServiceRole;
using EnrolmentStatusConst = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using PersonRoleConst = BackendAccountService.Data.DbConstants.PersonRole;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a producer organisation with an ApprovedPerson admin enrolment (Approved status).
/// Mirrors production by setting User.ExternalIdpUserId = UserId.
/// </summary>
public sealed class ProducerBuilder(TestBuilders builders)
{
    public async Task<BuiltEnrolment> Build()
    {
        var orgExternalId = Guid.NewGuid();
        Enrolment enrolment = null!;
        await builders.WithDbContextAsync(async ctx =>
        {
            enrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
                ctx, orgExternalId,
                ServiceRoleConst.Packaging.ApprovedPerson.Key,
                PersonRoleConst.Admin,
                EnrolmentStatusConst.Approved);
            enrolment.Connection.Person.User!.ExternalIdpUserId =
                enrolment.Connection.Person.User.UserId!.Value.ToString();
        }, save: true);
        return new BuiltEnrolment(enrolment);
    }
}
