namespace BackendAccountService.Data.Entities;

public class ComplianceScheme : DataEntity
{
    public int Id { get; set; }
    
    public string CompaniesHouseNumber { get; set; }
    
    public string Name { get; set; }
    
    public int? NationId { get; set; }

    public Nation? Nation { get; set; } = null!;
}