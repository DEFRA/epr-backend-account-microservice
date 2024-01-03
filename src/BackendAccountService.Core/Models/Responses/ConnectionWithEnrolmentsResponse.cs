namespace BackendAccountService.Core.Models.Responses;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public class ConnectionWithEnrolmentsResponse
{
    public PersonRole PersonRole { get; set; }
    public Guid UserId { get; set; }
    public ICollection<EnrolmentsFromConnectionResponse> Enrolments { get; init; }
}

public class EnrolmentsFromConnectionResponse
{
    public string ServiceRoleKey { get; set; }
    public EnrolmentStatus EnrolmentStatus { get; set; }
}