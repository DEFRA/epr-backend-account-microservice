using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class ConnectionWithEnrolmentsResponse
{
    public PersonRole PersonRole { get; set; }
    public Guid UserId { get; set; }
    public ICollection<EnrolmentsFromConnectionResponse> Enrolments { get; init; }
}

[ExcludeFromCodeCoverage]
public class EnrolmentsFromConnectionResponse
{
    public string ServiceRoleKey { get; set; }
    public EnrolmentStatus EnrolmentStatus { get; set; }
}