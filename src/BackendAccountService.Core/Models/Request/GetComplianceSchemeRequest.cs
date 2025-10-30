using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;

[ExcludeFromCodeCoverage]
public class GetComplianceSchemeRequest
{
    [Required]
    public int SelectedSchemeId { get; set; }
}