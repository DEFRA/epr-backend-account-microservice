namespace BackendAccountService.Core.Models.Responses;

public class OrganisationWithEnrolmentsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public OrganisationType Type { get; set; }
    public bool IsComplianceScheme { get; set; }
    public PersonRole PersonRole { get; set; }
    public ICollection<EnrolmentResponse> Enrolments { get; set; }
}