namespace BackendAccountService.Core.Models.Responses;

public class ComplianceSchemeMemberDto
{
    public Guid SelectedSchemeId { get; set; }
    public string OrganisationNumber { get; set; }
    public string OrganisationName { get; set; }
}
