using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IUserService
{
    Task<User?> GetUserByInviteAsync(string email, string inviteToken);
    Task<User?> GetUserByUserId(Guid userId);
    Task<Result<UserOrganisationsListModel>> GetUserOrganisationAsync(Guid userId, bool includeDeleted = false);
    Task<Result<IList<string>>> GetUserOrganisationIdListAsync(Guid userId);
    Task<Result<PersonWithOrganisationsResponse?>> GetPersonOrganisationsWithEnrolmentsForServiceAsync(Guid userId, string serviceKey);
    Task<UserModel?> GetApprovedUserUserByEmailAsync(string email);

    /// <summary>
    /// Checks if any users exist by their email address.
    /// </summary>
    /// <param name="userEmails">A distinct set of user emails to check.</param>
    /// <returns>The emails of users that do exist.</returns>
    Task<string[]> DoAnyUsersExist(IEnumerable<string> userEmails);

    Task<bool> InvitationTokenExists(string inviteToken);
    Task<Result<UpdateUserDetailsResponse>> UpdateUserDetailsRequest(Guid userId, Guid organisationExternalId, string serviceKey, UpdateUserDetailsRequest updateUserDetails);
    Task<Result<UserOrganisation>> GetSystemUserAndOrganisationAsync(string appUser);
    Task<Result<UserOrganisationsListModel>> GetUserOrganisationsWithEnrolmentsAsync(Guid userId, string serviceKey);

    Task<Guid?> GetUserIdByPersonId(Guid personId);
}