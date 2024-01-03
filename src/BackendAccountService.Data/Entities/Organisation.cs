using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class Organisation : DataEntity
{
    public int Id { get; set; }

    public int OrganisationTypeId { get; set; }
    
    public int? ProducerTypeId { get; set; }

    [MaxLength(30)]
    public string? CompaniesHouseNumber { get; set; }

    [MaxLength(160)]
    public string Name { get; set; }

    [MaxLength(170)]
    public string? TradingName { get; set; }

    [MaxLength(10)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(100)]
    public string? SubBuildingName { get; set; }

    [MaxLength(100)]
    public string? BuildingName { get; set; }

    [MaxLength(50)]
    public string? BuildingNumber { get; set; }

    [MaxLength(100)]
    public string? Street { get; set; }

    [MaxLength(100)]
    public string? Locality { get; set; }

    [MaxLength(100)]
    public string? DependentLocality { get; set; }

    [MaxLength(70)]
    public string? Town { get; set; }

    [MaxLength(50)]
    public string? County { get; set; }

    [MaxLength(54)]
    public string? Country { get; set; }

    [MaxLength(15)]
    public string? Postcode { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public bool IsComplianceScheme { get; set; }

    public int? NationId { get; set; }

    public OrganisationType OrganisationType { get; set; } = null!;

    public ProducerType? ProducerType { get; set; }

    public Nation? Nation { get; set; }
    
    public int? TransferNationId { get; set; }
    
    public Nation? TransferNation { get; set; }

    public ICollection<OrganisationsConnection> FromOrganisationConnections { get; set; } = null!;
    
    public ICollection<OrganisationsConnection> ToOrganisationsConnections { get; set; } = null!;

    public ICollection<PersonOrganisationConnection> PersonOrganisationConnections { get; set; } = null!;
}
