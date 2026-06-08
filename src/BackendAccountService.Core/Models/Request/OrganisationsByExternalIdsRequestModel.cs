using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request;

public class OrganisationsByExternalIdsRequestModel
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one external id must be supplied.")]
    public IList<Guid> ExternalIds { get; init; } = new List<Guid>();
}
