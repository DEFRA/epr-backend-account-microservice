namespace BackendAccountService.Core.Models;

public class ProducerComplianceSchemeDto
{
    public Guid SelectedSchemeId { get; set; }
    
    public string ComplianceSchemeName { get; set; }
    
    public Guid ComplianceSchemeId { get; set; }
    
    public string ComplianceSchemeOperatorName { get; set; }
    
    public Guid ComplianceSchemeOperatorId { get; set; }
}