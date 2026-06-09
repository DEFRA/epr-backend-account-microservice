using BackendAccountService.Data.Entities;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Bundle returned by the <see cref="ProducerBuilder"/> / <see cref="RegulatorBuilder"/> /
/// <see cref="SchemeOperatorWithAdminBuilder"/> build paths. Exposes the load-bearing
/// identifiers as properties so callers pull just what they need without destructuring.
/// </summary>
public sealed record BuiltEnrolment(Enrolment Enrolment)
{
    public Guid UserId => Enrolment.Connection.Person.User!.UserId!.Value;
    public Guid OrgExternalId => Enrolment.Connection.Organisation.ExternalId;
    public Person Person => Enrolment.Connection.Person;
    public Organisation Organisation => Enrolment.Connection.Organisation;
}
