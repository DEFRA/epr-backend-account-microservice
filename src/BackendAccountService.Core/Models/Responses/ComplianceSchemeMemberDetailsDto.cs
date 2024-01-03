namespace BackendAccountService.Core.Models.Responses;

public class ComplianceSchemeMemberDetailDto
{ 
    public string OrganisationName { get; set; }
    public string OrganisationNumber { get; set; }
    public Nation RegisteredNation { get; set; }
    public string ComplianceScheme { get; set; }
    public ProducerType? ProducerType { get; set; }
    public string? CompanyHouseNumber { get; set; }
    
}