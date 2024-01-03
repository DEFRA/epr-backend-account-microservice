namespace BackendAccountService.Core.Models.Responses;

public class PersonWithOrganisationsResponse
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public ICollection<OrganisationWithEnrolmentsResponse> Organisations { get; set; }
}