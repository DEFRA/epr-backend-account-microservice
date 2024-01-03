using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.Entities;

public class User : ISoftDeletableEntity
{
    public int Id { get; set; }

    [Required]
    public Guid? UserId { get; set; }

    [MaxLength(200)]
    [Comment("External Provider Identity ID")]
    public string? ExternalIdpId { get; set; }

    [MaxLength(200)]
    [Comment("External Provider Identity User ID")]
    public string? ExternalIdpUserId { get; set; }

    [MaxLength(254)]
    public string? Email { get; set; }

    public Person Person { get; set; } = null!;
    
    public bool IsDeleted { get; set; }
    
    [MaxLength(100)]
    public string? InviteToken { get; set; }
    
    [MaxLength(254)]
    public string? InvitedBy { get; set; }
}
