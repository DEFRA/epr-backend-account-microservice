using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class TerminateSubsidiaryModel
{
    public Guid ParentExternalId { get; init; }

    public int ParentOrganisationId { get; init; }

    public int ChildOrganisationId { get; init; }

    public Guid UserExternalId { get; set; }

    public int UserId { get; set; }
}
