namespace BackendAccountService.Core.Models.Request;

public class ApprovedUserRequest
{
    public Guid UserId { get; set; }
    
    public Guid OrganisationId { get; set; }
    
    public Guid RemovedConnectionExternalId { get; set; }
    
    public Guid PromotedPersonExternalId { get; set; }
}