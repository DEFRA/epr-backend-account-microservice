using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.ValidationData.Api.Models;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetailsResponse
{
    public List<SubsidiaryOrganisationDetail> SubsidiaryOrganisationDetails { get; set; }
}