using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class BulkSubsidiaryTerminateRequestModel
{
    public string? ParentOrganisationId { get; init; }

    public string? ChildOrganisationId { get; init; }

    public Guid ParentOrganisationExternalId { get; init; }

    public Guid UserId { get; set; }
}
