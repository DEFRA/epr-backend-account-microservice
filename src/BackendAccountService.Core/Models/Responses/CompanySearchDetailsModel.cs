namespace BackendAccountService.Core.Models.Responses;

public class CompanySearchDetailsModel
{
    public Company Company { get; set; }
    public IEnumerable<UserInformation> CompanyUserInformation {get; set;}
}
