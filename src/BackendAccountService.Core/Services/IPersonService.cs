using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Services
{
    public interface IPersonService
    {
        Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId);
        
        Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId);
    }
}