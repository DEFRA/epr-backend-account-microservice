namespace BackendAccountService.Core.Services;

public interface IValidationService
{
    Task<bool> IsUserInvitedAsync(string email);
    bool IsAuthorisedToManageComplianceScheme(Guid userId, Guid organisationId);
    Task<bool> IsAuthorisedToViewComplianceSchemeMembers(Guid userId, Guid organisationId);
    bool IsAuthorisedToManageUsers(Guid userId, Guid organisationId, int serviceRoleId);
    Task<bool> IsAuthorisedToManageUsersFromOrganisationForService(Guid userId, Guid organisationId, string serviceKey);
    Task<bool> IsAuthorisedToManageDelegatedUsersFromOrganisationForService(Guid userId, Guid organisationId, string serviceKey);
    bool IsAuthorisedToRemoveEnrolledUser(Guid loggedInUserId, Guid organisationId, int serviceRoleId, Guid enrolledPersonId);
    Task<string> UserInvitedTokenAsync(Guid? userId);
}