using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeMemberRemovalAuditLog 
{
    public int Id { get; set; }
    
    public int SchemeOrganisationId { get; set; }
    
    public int MemberOrganisationId { get; set; }

    public int ComplianceSchemeId { get; set; }

    public Guid RemovedBy { get; set; }

    public DateTimeOffset RemovedOn { get; set; }

    [MaxLength(2000)]
    public string? ReasonDescription { get; set; }      
    
    public Organisation SchemeOrganisation { get; set; }

    public Organisation MemberOrganisation { get; set; }

    public ComplianceScheme ComplianceScheme { get; set; }
}