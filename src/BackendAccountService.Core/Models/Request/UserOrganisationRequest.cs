using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class UserOrganisationRequest
{
    [Required]
    public Guid UserOid { get; set; }

    [Required]
    public Guid OrganisationId { get; set; }
}