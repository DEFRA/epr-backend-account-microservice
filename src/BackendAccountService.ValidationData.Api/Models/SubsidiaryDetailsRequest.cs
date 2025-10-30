using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.ValidationData.Api.Models;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetailsRequest
{
    public List<SubsidiaryOrganisationDetail> SubsidiaryOrganisationDetails { get; set; }
}