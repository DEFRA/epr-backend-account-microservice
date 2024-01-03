namespace BackendAccountService.Core.Services;

public interface ITokenService
{
    string GenerateInviteToken();
}