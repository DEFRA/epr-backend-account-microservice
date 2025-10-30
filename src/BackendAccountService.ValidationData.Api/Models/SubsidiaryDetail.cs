using System;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.ValidationData.Api.Models;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetail
{
    public string ReferenceNumber { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public bool SubsidiaryExists { get; set; }

    public bool SubsidiaryBelongsToAnyOtherOrganisation { get; set; }

    public bool SubsidiaryDoesNotBelongToAnyOrganisation { get; set; }
}