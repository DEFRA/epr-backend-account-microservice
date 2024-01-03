namespace BackendAccountService.Core.Models.Request;

using System.ComponentModel.DataAnnotations;

public class RemoveComplianceSchemeRequest
{
    [Required]
    public Guid SelectedSchemeId { get; set; }

    [Required]
    public Guid UserOId { get; set; }

    [Required]
    public Guid OrganisationId { get; set; }
}