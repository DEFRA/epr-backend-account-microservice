using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public record ReExAddOrganisationResponse
{
    public required string UserFirstName { get; set; }
    public required string UserLastName { get; set; }
    public required IEnumerable<ServiceRoleResponse> UserServiceRoles { get; set; }

    public required Guid OrganisationId { get; set; }
    public required string ReferenceNumber { get; set; }

    public required IEnumerable<InvitedApprovedUserResponse> InvitedApprovedUsers { get; set; }
}