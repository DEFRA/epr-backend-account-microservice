using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class UserModel
{
    public int? Id { get; set; }
    
    [Required]
    public Guid? UserId { get; set; }

    [MaxLength(200)]
    public string? ExternalIdpId { get; set; }

    [MaxLength(200)]
    public string? ExternalIdpUserId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = null!;
}