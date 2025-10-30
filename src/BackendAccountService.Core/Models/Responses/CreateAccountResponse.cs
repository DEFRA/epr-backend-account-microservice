using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public record CreateAccountResponse
{
    public Guid OrganisationId { get; set; }
    public string ReferenceNumber { get; set; } = null!;
}