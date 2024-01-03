using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeMemberRemovalAuditLogsReason
{
    public int Id { get; set; }

    public int AuditLogId { get; set; }

    public int ReasonId { get; set; }

    public ComplianceSchemeMemberRemovalAuditLog AuditLog { get; set; }

    public ComplianceSchemeMemberRemovalReason Reason { get; set; }
}