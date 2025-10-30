using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterOrganisationModel
{
    public OrganisationType OrganisationType { get; set; }

    public ProducerType? ProducerType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = null!;

    [MaxLength(170)]
    public string? TradingName { get; set; }

    [Required]
    public AddressModel Address { get; set; } = null!;

    public bool ValidatedWithCompaniesHouse { get; set; }

    public bool IsComplianceScheme { get; set; }

    public string? ReferenceNumber { get; set; }

    public Guid? ExternalId { get; set; }

    public Nation? Nation { get; set; }

    public OrganisationRelationshipModel? OrganisationRelationship { get; set; }
}