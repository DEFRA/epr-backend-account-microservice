using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class SubsidiaryTerminateRequestModel
{
    public Guid ParentOrganisationId { get; init; }

    public Guid ChildOrganisationId { get; init; }

    public Guid UserId { get; set; }
}
