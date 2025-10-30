using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;
[ExcludeFromCodeCoverage]
public class ManageUserDetailsChangeRequest
{
    public bool HasRegulatorAccepted { get; set; } = false;
    public string? RegulatorComment { get; set; }
}