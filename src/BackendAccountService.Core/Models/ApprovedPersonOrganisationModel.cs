using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class ApprovedPersonOrganisationModel
{
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

    [MaxLength(15)]
    public string? Postcode { get; set; }

    [MaxLength(54)]
    public string? Country { get; set; }
    
    [MaxLength(100)]
    public string? OrganisationName { get; set; }
    
    public string? ApprovedUserEmail { get; set; }
    }