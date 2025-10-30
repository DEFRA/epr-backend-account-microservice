using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class OrganisationUserDetailChangeRequest
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public Guid ChangeHistoryExternalId { get; set; }
    public DateTimeOffset DeclarationDate { get; set; }
    public string? ServiceRole { get; set; } = string.Empty;
    public int? ServiceRoleId { get; set; }
}