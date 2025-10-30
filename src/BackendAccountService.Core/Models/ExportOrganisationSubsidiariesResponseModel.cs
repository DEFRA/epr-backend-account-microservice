namespace BackendAccountService.Core.Models;
public class ExportOrganisationSubsidiariesResponseModel
{
    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public string OrganisationName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public DateTimeOffset? JoinerDate { get; set; }

    public string ReportingType { get; set; }
}