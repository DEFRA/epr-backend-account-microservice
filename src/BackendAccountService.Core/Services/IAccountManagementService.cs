using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IAccountManagementService
{
    Task<string> CreateInviteeAccountAsync(AddInviteUserRequest request);
    Task<bool> EnrolInvitedUserAsync(User user, EnrolInvitedUserRequest request);
    Task<bool> EnrolReInvitedUserAsync(User user);
    Task<string> ReInviteUserAsync(InvitedUser invitedUser, InvitingUser invitingUser);
}