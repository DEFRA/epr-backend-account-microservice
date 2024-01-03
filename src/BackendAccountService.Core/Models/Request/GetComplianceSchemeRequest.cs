namespace BackendAccountService.Core.Models.Request;

using System.ComponentModel.DataAnnotations;

public class GetComplianceSchemeRequest
{
    [Required]
    public int SelectedSchemeId { get; set; }
}

