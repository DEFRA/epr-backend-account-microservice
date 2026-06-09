using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class OrganisationsByExternalIdsResponse
{
    public IList<OrganisationLookupModel> Organisations { get; init; } = new List<OrganisationLookupModel>();

    public IList<Guid> NotFoundExternalIds { get; init; } = new List<Guid>();
}
