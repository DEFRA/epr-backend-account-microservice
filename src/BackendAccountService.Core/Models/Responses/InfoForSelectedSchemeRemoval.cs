using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class InfoForSelectedSchemeRemoval
{
    public string ComplianceSchemeName { get; set; }
    public Nation ComplianceSchemeNation { get; set; }
    public string OrganisationName { get; set; }
    public Nation OrganisationNation { get; set; }
    public string? OrganisationNumber { get; set; }
    public List<EmailRecipient> RemovalNotificationRecipients { get; set; }
}