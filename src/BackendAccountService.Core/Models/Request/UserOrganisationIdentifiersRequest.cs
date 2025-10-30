using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class UserOrganisationIdentifiersRequest
{
    [Required]
    public Guid ObjectId { get; set; }
}