using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request;

public class AddInviteUserRequest
{
    [Required]
    public InvitedUser InvitedUser { get; set; }

    [Required]
    public InvitingUser InvitingUser { get; set; }
}

public class InvitedUser
{
    [EmailAddress]
    [MaxLength(100)]
    [Required]
    public string Email { get; set; }

    [Required]
    public int PersonRoleId { get; set; }

    [Required]
    public int ServiceRoleId { get; set; }

    [Required]
    public Guid OrganisationId { get; set; }

    public Guid? UserId { get; set; } = Guid.Empty!;
}

public class InvitingUser
{
    [EmailAddress]
    [MaxLength(100)]
    [Required]
    public string Email { get; set; }

    [Required]
    public Guid UserId { get; set; }
}