using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IUserService
{
    Task<User?> GetUserByInviteAsync(string email, string inviteToken);
    Task<Result<UserOrganisationsListModel>> GetUserOrganisationAsync(Guid userId);
    Task<Result<PersonWithOrganisationsResponse?>> GetPersonOrganisationsWithEnrolmentsForServiceAsync(Guid userId, string serviceKey);
    Task<UserModel?> GetApprovedUserUserByEmailAsync(string email);
}