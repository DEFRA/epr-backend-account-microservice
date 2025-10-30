using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class BulkSubsidiaryAddRequestModel
{
    public string? ParentOrganisationId { get; init; }

    public string? ChildOrganisationId { get; init; }

    public Guid ParentOrganisationExternalId { get; init; }

    public Guid ChildOrganisationExternalId { get; init; }

    public Guid UserId { get; set; }

    public string? JoinerDate { get; set; }

    public int? LeaverCodeId { get; set; }

    public string? LeaverDate { get; set; }

    public string? OrganisationChangeReason { get; set; }
}
