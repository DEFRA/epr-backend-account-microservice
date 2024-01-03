using System.ComponentModel.DataAnnotations;
using BackendAccountService.Core.Models.Request.Attributes;

namespace BackendAccountService.Core.Models.Request;

public class EnrolInvitedUserRequest
{
    [Required]
    [NotDefault]
    public Guid UserId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string InviteToken { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } 
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
}