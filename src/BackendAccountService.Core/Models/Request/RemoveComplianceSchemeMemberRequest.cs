using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request;

public class RemoveComplianceSchemeMemberRequest
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; }

    [MaxLength(2000)]
    public string? TellUsMore { get; set; }
}