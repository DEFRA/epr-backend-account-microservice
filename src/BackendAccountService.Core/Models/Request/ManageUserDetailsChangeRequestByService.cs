using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;
[ExcludeFromCodeCoverage]
public class ManageUserDetailsChangeRequestByService
{
    public string OrganisationReference { get; set; }

    public string UserEmail { get; set; }

    public string RegulatorEmail { get; set; }

    public string? RegulatorComment { get; set; }

    public bool HasRegulatorAccepted { get; set; } = false;

}