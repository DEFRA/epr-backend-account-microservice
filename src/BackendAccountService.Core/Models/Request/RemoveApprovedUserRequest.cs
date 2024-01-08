namespace BackendAccountService.Core.Models.Request;

public class RemoveApprovedUserRequest
{
    public Guid UserId { get; set; }
    
    public Guid OrganisationId { get; set; }
    
    public Guid ConnectionExternalId { get; set; }
}