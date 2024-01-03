namespace BackendAccountService.Core.Models.Request;

using System.ComponentModel.DataAnnotations;

public class UpdateSelectedComplianceSchemeRequest
{
    [Required]
    public Guid SelectedSchemeId { get; set; }
    
    [Required]
    public Guid ComplianceSchemeId { get; set; }
    
    [Required]
    public Guid ProducerOrganisationId { get; set; }
    
    [Required]
    public Guid UserOid { get; set; }
}