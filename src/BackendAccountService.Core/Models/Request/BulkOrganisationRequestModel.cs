using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class BulkOrganisationRequestModel
{
    public OrganisationModel Subsidiary { get; init; }
    public Guid ParentOrganisationId { get; init; }
    public Guid UserId { get; init; }
    public HttpStatusCode? StatusCode { get; set; }
}
