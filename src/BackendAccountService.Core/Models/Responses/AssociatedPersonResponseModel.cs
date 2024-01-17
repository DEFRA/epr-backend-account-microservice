namespace BackendAccountService.Core.Models.Responses;
public class AssociatedPersonResponseModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string OrganisationId { get; set; }
    public string CompanyName { get; set; }
    public int ServiceRoleId { get; set; }
     
    public EmailNotificationType EmailNotificationType { get; set; }
}
