using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class RelationshipResponseModel
{
    public Guid? ParentOrganisationExternalId { get; set; }

    public string OrganisationNumber { get; set; }

    public string OrganisationName { get; set; }

    public string RelationshipType { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public DateTimeOffset? JoinerDate { get; set; }

    public string? LeaverCode { get; set; }

    public DateTimeOffset? LeaverDate { get; set; }

    public string? OrganisationChangeReason { get; set; }
}