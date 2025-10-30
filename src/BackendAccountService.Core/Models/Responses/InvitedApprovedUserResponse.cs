using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public record InvitedApprovedUserResponse
{
    public required string Email { get; set; }
    public required string InviteToken { get; set; }
    public required ServiceRoleResponse ServiceRole { get; set; }
}