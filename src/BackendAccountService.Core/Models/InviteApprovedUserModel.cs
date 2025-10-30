using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Result;

[ExcludeFromCodeCoverage]
public class InviteApprovedUserModel
{
    public string ServiceRoleId { get; set; }
    public string CompanyHouseNumber { get; set; }
    public string OrganisationId { get; set; }
    public string Email { get; set; }

    public bool IsInvitationTokenInvalid { get; set; }
}