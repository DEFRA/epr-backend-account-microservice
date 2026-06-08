using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class OrganisationLookupModel
{
    public Guid ExternalId { get; init; }

    public string Name { get; init; }
}
