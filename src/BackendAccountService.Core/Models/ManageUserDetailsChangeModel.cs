using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;
[ExcludeFromCodeCoverage]
public class ManageUserDetailsChangeModel
{
    public Guid UserId { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid ChangeHistoryExternalId { get; set; }

    public bool HasRegulatorAccepted { get; set; } = false;

    public string? RegulatorComment { get; set; }

}