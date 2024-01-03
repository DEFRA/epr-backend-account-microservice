using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Core.Services
{
    public interface IRegulatorOrganisationService
    {
        Task<CheckOrganisationExistResponseModel?> GetRegulatorOrganisationByNationAsync(string nation);

        Task<Result<CreateRegulatorOrganisationResponse>> CreateNewRegulatorOrganisationAsync(CreateRegulatorOrganisationRequest request);
    }
}
