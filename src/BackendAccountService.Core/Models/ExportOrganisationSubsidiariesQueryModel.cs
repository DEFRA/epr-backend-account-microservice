using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ExportOrganisationSubsidiariesQueryModel
{
    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public string ParentOrganisationName { get; set; }
    
    public string ChildOrganisationName { get; set; }

    public string ParentCompaniesHouseNumber { get; set; }

    public string ChildCompaniesHouseNumber { get; set; }

    public DateTimeOffset? ChildJoinerDate { get; set; }
}