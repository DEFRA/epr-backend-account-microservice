namespace BackendAccountService.Core.Models.Responses;

public class OrganisationPersonDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string JobTitle { get; set; }
    public string Email { get; set; }
    public string TelephoneNumber { get; set; }
    public string ServiceRole { get; set; }
}
