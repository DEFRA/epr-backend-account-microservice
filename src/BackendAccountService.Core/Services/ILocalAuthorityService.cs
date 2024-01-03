using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Core.Services
{
    public interface ILocalAuthorityService
    {
        Task<Result<LocalAuthorityResponseModel>> CreateNewLocalAuthorityOrganisationAsync(
            CreateLocalAuthorityRequest request);

        Task<IList<LocalAuthorityResponseModel>> GetLocalAuthorityOrganisationAsync();

        Task<IList<LocalAuthorityResponseModel>>
            GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(int id);

        Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityByDistrictCodeAsync(string districtCode);
        Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityOrganisationByExternalIdAsync(string id);
        Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityByOrganisationNameAsync(string name);
        Task<Result> RemoveLocalAuthorityByDistrictCodeAsync(RemoveLocalAuthorityRequest request);
        Task<Result> RemoveLocalAuthorityByExternalIdAsync(RemoveLocalAuthorityRequest request);

        Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByDistrictCodeAsync(
            UpdateLocalAuthorityRequest request);

        Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByExternalIdAsync(
            UpdateLocalAuthorityRequest request);
        
        Task<Result> GetLocalAuthorityOrganisationNationAsync(string nationName);
        Task<Result> GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(string organisationTypeName);
    }
}