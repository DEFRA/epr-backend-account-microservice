using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IPersonService
{
    Task<Person?> GetPersonByUserId(Guid userId);

    Task<PersonResponseModel?> GetPersonResponseByUserId(Guid userId);

    Task<PersonResponseModel?> GetAllPersonByUserIdAsync(Guid userId);

    Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId);
        
    Task<InviteApprovedUserModel> GetPersonServiceRoleByInviteTokenAsync(string token);
}