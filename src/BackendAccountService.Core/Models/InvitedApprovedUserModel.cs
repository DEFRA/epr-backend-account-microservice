namespace BackendAccountService.Core.Models;

public class InvitedApprovedUserModel
{
    public PersonModel Person { get; set; }

    /// <summary>
    /// officer_role in Companies House API
    /// </summary>
    public string? JobTitle { get; set; }
}