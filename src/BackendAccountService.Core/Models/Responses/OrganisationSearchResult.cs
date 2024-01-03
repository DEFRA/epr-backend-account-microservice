namespace BackendAccountService.Core.Models.Responses;

public class OrganisationSearchResult
{
    public string OrganisationName { get; set; }
    public bool IsComplianceScheme { get; set; }
    public string OrganisationId { get; set; }
    public Guid ExternalId { get; set; }
    public string CompanyHouseNumber { get; set; }
}