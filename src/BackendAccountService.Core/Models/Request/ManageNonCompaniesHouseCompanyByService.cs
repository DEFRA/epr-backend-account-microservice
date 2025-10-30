using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request;
[ExcludeFromCodeCoverage]
public class ManageNonCompaniesHouseCompanyByService
{
    public string OrganisationReference { get; set; }

    public string UserEmail { get; set; }

    [MaxLength(100)]
    public string OrganisationName { get; set; }

    [MaxLength(50)]
    public string BuildingNumber { get; set; }

    [MaxLength(100)]
    public string Street { get; set; }

    [MaxLength(15)]
    public string Postcode { get; set; }

    [MaxLength(70)]
    public string Town { get; set; }

    [MaxLength(100)]
    public string? FlatOrApartmentNumber { get; set; }

    [MaxLength(100)]
    public string? BuildingName { get; set; }

    [MaxLength(50)]
    public string? County { get; set; }

}