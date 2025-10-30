using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.ValidationData.Api.Models;

[ExcludeFromCodeCoverage]
public class SubsidiaryOrganisationDetail
{
    public string OrganisationReference { get; set; }

    public List<SubsidiaryDetail> SubsidiaryDetails { get; set; }
}