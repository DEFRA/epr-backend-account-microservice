using System;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class LinkOrganisationRequestModel
{
    public OrganisationModel Subsidiary { get; init; }
    public Guid ParentOrganisationId { get; init; }
    public Guid UserId { get; init; }
}
