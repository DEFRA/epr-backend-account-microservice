using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class AcceptNominationForApprovedPersonRequest
{
    [Required]
    [MaxLength(450)]
    public string? JobTitle { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Telephone { get; set; }

    [Required]
    [MaxLength(450)]
    public string? DeclarationFullName { get; set; }

    [Required]
    public DateTime? DeclarationTimeStamp { get; set; }
}
