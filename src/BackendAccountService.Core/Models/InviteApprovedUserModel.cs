namespace BackendAccountService.Core.Models.Result;

public class InviteApprovedUserModel
{
    public string ServiceRoleId { get; set; }
    public string CompanyHouseNumber { get; set; }
    public string OrganisationId { get; set; }
    public string Email { get; set; }
}