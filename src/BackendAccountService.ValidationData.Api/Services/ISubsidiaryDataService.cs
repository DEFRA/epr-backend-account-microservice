using BackendAccountService.ValidationData.Api.Models;
using System.Threading.Tasks;

namespace BackendAccountService.ValidationData.Api.Services;

public interface ISubsidiaryDataService
{
    Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest);
}