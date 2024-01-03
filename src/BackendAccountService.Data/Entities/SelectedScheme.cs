namespace BackendAccountService.Data.Entities;

public class SelectedScheme : DataEntity
{
    public int Id { get; set; }
    
    public int OrganisationConnectionId { get; set; }

    public int ComplianceSchemeId { get; set; }
    
    public OrganisationsConnection OrganisationConnection { get; set; } = null!;

    public ComplianceScheme ComplianceScheme { get; set; } = null!;
}