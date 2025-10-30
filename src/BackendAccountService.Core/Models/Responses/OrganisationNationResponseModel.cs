using System.Diagnostics.CodeAnalysis;
using BackendAccountService;
using BackendAccountService.Core;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class OrganisationNationResponseModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NationCode { get; set; }
}
