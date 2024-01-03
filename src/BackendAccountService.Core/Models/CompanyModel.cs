namespace BackendAccountService.Core.Models;

public class Company
{   
    public string OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public int OrganisationTypeId { get; set; }
    public string? CompaniesHouseNumber { get; set; }
    public bool IsComplianceScheme { get; set; }
    public AddressModel RegisteredAddress { get; set; }
}