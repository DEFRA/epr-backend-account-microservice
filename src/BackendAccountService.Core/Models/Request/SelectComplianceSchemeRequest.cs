namespace BackendAccountService.Core.Models.Request;

using System.ComponentModel.DataAnnotations;

public class SelectComplianceSchemeRequest
{
    [Required]
    public Guid ComplianceSchemeId { get; set; }
    
    [Required]
    public Guid ProducerOrganisationId { get; set; }
    
    [Required]
    public Guid UserOId { get; set; }
}