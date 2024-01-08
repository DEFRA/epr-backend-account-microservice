using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Core.Services
{
    public interface IPersonService
    {
        Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId);
        
        Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId);
        
        Task<InviteApprovedUserModel> GetPersonServiceRoleByInviteTokenAsync(string token);
        
    }
}