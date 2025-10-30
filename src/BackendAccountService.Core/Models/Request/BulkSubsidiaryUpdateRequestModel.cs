using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class BulkSubsidiaryUpdateRequestModel
{
    public string? ParentOrganisationId { get; init; }

    public string? ChildOrganisationId { get; init; }

    public Guid ParentOrganisationExternalId { get; init; }

    public Guid ChildOrganisationExternalId { get; init; }

    public Guid UserId { get; set; }

    public DateTimeOffset? JoinerDate { get; set; }

    public string? LeaverCode { get; set; }

    public DateTimeOffset? LeaverDate { get; set; }

    public string? OrganisationChangeReason { get; set; }
}