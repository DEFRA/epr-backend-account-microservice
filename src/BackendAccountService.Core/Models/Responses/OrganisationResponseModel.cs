using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class OrganisationResponseModel : OrganisationModel
{
    public DateTimeOffset CreatedOn { get; set; }

    public int Id { get; set; }
}
