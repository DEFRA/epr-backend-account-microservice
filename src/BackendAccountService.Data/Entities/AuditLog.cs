using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public class AuditLog
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OrganisationId { get; set; }

    [MaxLength(100)]
    public string? ServiceId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    [MaxLength(100)]
    public string Entity { get; set; } = null!;

    [MaxLength(100)]
    public string Operation { get; set; } = null!;

    public int? InternalId { get; set; }

    public Guid? ExternalId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? Changes { get; set; }
}