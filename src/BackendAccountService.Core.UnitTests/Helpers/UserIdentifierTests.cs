using BackendAccountService.Core.Helpers;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;
using FluentAssertions;

namespace BackendAccountService.Core.UnitTests.Helpers;

[TestClass]
public class UserIdentifierTests
{
    [TestMethod]
    public void FromUser_WhenUserIdIsSet_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = 42, UserId = userId };

        var result = UserIdentifier.FromUser(user);

        result.Should().Be(userId.ToString());
    }

    [TestMethod]
    public void FromUser_WhenUserIdIsNull_ReturnsDatabaseId()
    {
        var user = new User { Id = 42, UserId = null };

        var result = UserIdentifier.FromUser(user);

        result.Should().Be("42");
    }

    [TestMethod]
    public void FromUser_WhenUserIdIsEmpty_ReturnsDatabaseId()
    {
        var user = new User { Id = 99, UserId = Guid.Empty };

        var result = UserIdentifier.FromUser(user);

        result.Should().Be("99");
    }

    [TestMethod]
    public void FromInvitedUser_WhenUserIdIsSet_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var invitedUser = new InvitedUser { UserId = userId };

        var result = UserIdentifier.FromInvitedUser(invitedUser);

        result.Should().Be(userId.ToString());
    }

    [TestMethod]
    public void FromInvitedUser_WhenUserIdIsNull_ReturnsUnspecified()
    {
        var invitedUser = new InvitedUser { UserId = null };

        var result = UserIdentifier.FromInvitedUser(invitedUser);

        result.Should().Be("unspecified-user-id");
    }

    [TestMethod]
    public void FromInvitedUser_WhenUserIdIsEmpty_ReturnsUnspecified()
    {
        var invitedUser = new InvitedUser { UserId = Guid.Empty };

        var result = UserIdentifier.FromInvitedUser(invitedUser);

        result.Should().Be("unspecified-user-id");
    }
}
