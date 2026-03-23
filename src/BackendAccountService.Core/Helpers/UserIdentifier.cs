using System.Globalization;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Helpers;

public static class UserIdentifier
{
    public static string FromUser(User user)
    {
        if (user.UserId is { } uid && uid != Guid.Empty)
        {
            return uid.ToString();
        }

        return user.Id.ToString(CultureInfo.InvariantCulture);
    }

    public static string FromInvitedUser(InvitedUser invitedUser)
    {
        if (invitedUser.UserId is { } uid && uid != Guid.Empty)
        {
            return uid.ToString();
        }

        return "unspecified-user-id";
    }
}
