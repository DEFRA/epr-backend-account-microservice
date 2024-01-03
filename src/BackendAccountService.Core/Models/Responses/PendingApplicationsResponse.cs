namespace BackendAccountService.Core.Models.Responses;

public class PendingEnrolmentFlags
{
    public bool HasApprovedPending { get; set; }
    
    public bool HasDelegatePending { get; set; }
}

public class OrganisationEnrolments
{
    public Guid OrganisationId { get; set; }
    
    public string OrganisationName { get; set; }
    
    public DateTime LastUpdate { get; set; }
    
    public PendingEnrolmentFlags Enrolments { get; set; }
}