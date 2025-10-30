using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class PartnerModel
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string PartnerRole { get; set; }
}