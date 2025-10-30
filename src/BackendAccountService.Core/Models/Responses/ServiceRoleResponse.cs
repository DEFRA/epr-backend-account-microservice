using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public record ServiceRoleResponse
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}