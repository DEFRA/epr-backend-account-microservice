using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeMembershipModel
{
    public Guid SelectedSchemeId { get; set; }

    public Guid ProducerOrganisationId { get; set; }

    public string ProducerOrganisationName { get; set; }

    public string? ProducerOrganisationNumber { get; set; }

    public Guid ComplianceSchemeId { get; set; }

    public string ComplianceSchemeName { get; set; }

    public Guid SchemeOrganisationId { get; set; }

    public string SchemeOrganisationName { get; set; }

    public int? NationId { get; set; }

    public DateTimeOffset MembershipStartDate { get; set; }

    public DateTimeOffset? MembershipEndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string ChangeType { get; set; }

    public Guid? RemovedBy { get; set; }

    public string? RemovalReasonCode { get; set; }

    public string? RemovalReason { get; set; }

    public string? RemovalReasonDescription { get; set; }

    public string? ChangeSource { get; set; }
}
