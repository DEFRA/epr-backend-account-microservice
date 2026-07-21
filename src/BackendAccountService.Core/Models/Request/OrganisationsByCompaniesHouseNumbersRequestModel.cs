using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request;

public class OrganisationsByCompaniesHouseNumbersRequestModel
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one companies house number must be supplied.")]
    public IList<string> CompaniesHouseNumbers { get; init; } = new List<string>();
}
