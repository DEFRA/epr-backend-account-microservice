using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class UserModel
{
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