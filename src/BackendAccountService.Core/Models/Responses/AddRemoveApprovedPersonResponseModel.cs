namespace BackendAccountService.Core.Models.Responses;

public class AddRemoveApprovedPersonResponseModel
{
    public List<AssociatedPersonResponseModel> DemotedBasicUsers { get; set; } =
        new ();
    public string InviteToken { get; set; }
    public string OrganisationReferenceNumber { get; set; }
    public string OrganisationName { get; set; }
}